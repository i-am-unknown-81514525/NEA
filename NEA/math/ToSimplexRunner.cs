using NEA.components;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ui.math;
using System;
using math_parser.tokenizer;
using math_parser.atom;
using System.Linq;
using NEA.Math;

// ^[a-zA-Z][a-zA-Z0-9]*(?:_[a-zA-Z0-9]+)?$

namespace NEA.math
{
    struct Meta
    {
        public readonly int? artifical;
        public readonly int slack;
        public readonly EqResult result;

        public Meta(int? artifical, int slack, EqResult result)
        {
            this.artifical = artifical;
            this.slack = slack;
            this.result = result;
        }
    }

    public static class ToSimplexRunner
    {

        public static bool IsArtificialVariable(string name)
        {
            return IsValidVariableName(name) && name.StartsWith("a_");
        }

        public static bool IsSlackVariable(string name)
        {
            return IsValidVariableName(name) && name.StartsWith("s_");
        }

        public static bool IsValidVariableName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9]*(?:_[a-zA-Z0-9]+)?$");
        }

        public static SimplexInterationRunner Translate(string model)
        {
            Keyword.PushKeyword("MAX"); // TODO: lib fix to prevent repeat Keyword
            Keyword.PushKeyword("ST");
            Keyword.PushKeyword("END");

            TokenSequence<ParseResult> tree = new TokenSequence<ParseResult>(
                new Literal("MAX"),
                new PotentialSpace(),
                new Expression(),
                new PotentialNewLine(),
                new Literal("ST"),
                new PotentialNewLine(),
                new Repeat<TokenSequenceResult<ParseResult>>(new TokenSequence<ParseResult>(new Equation(), new PotentialNewLine()), 1, 8),
                new Literal("END")
            );

            CharacterStream stream = new CharacterStream(model.Trim());
            TokenSequenceResult<ParseResult> result;

            try
            {
                result = tree.Parse(stream);
            }
            catch (TokenParseException e)
            {
                throw new SimplexError($"Failed to parse model input: {e.Message}");
            }
            catch (PrattParseError e)
            {
                throw new SimplexError($"Pratt Parse failure: {e.Message}");
            }
            catch (DivideByZeroException e)
            {
                throw new SimplexError($"Divide by zero error when parsing model input: {e.Message}");
            }

            if (!stream.IsEof)
            {
                throw new SimplexError("Not End of line when parsing final token");
            }
            

            ExprResult objective = (ExprResult)result.parseResult[2];

            RepeatListResult<TokenSequenceResult<ParseResult>> eqs_token = (RepeatListResult<TokenSequenceResult<ParseResult>>)result.parseResult[6];

            List<EqResult> eqs = new List<EqResult>();

            foreach (TokenSequenceResult<ParseResult> res in eqs_token)
            {
                eqs.Add((EqResult)res.parseResult[0]);
            }
            return Translate(objective, eqs.ToArray());
        }

        public static SimplexInterationRunner Translate((ExprResult optimal, EqResult[] constraints) inputs) => Translate(inputs.optimal, inputs.constraints);

        public static SimplexInterationRunner Translate(ExprResult optimal, EqResult[] constraints)
        {
            List<string> varNames = new List<string>();
            int used_artifical = 0;
            int used_slack = 0;
            foreach (Term term in optimal.terms)
            {
                if (term.term_name == "") continue;
                if (!IsValidVariableName(term.term_name)) {
                    throw new SimplexError($"Invalid variable name: {term.term_name}");
                }
                if (IsArtificialVariable(term.term_name) || IsSlackVariable(term.term_name)) {
                    throw new SimplexError($"Invalid variable name: {term.term_name}");
                }
                if (varNames.Contains(term.term_name)) {
                    throw new SimplexError($"Duplicate variable name: {term.term_name}");
                }
                varNames.Add(term.term_name);
            }

            List<Meta> construct_meta = new List<Meta>();
            foreach (EqResult eq in constraints)
            {
                Fraction lhs_literal = 0; // LHS, which is opposite to RHS
                foreach (Term t in eq.exprs.terms)
                {
                    if (t.term_name == "")
                    {
                        lhs_literal += t.coefficient.Transitivity();
                    } else
                    {
                        if (!IsValidVariableName(t.term_name)) {
                            throw new SimplexError($"Invalid variable name: {t.term_name}");
                        }
                        if (IsArtificialVariable(t.term_name) || IsSlackVariable(t.term_name)) {
                            throw new SimplexError($"Invalid variable name: {t.term_name}");
                        }
                        if (!varNames.Contains(t.term_name))
                        {
                            varNames.Add(t.term_name);
                        }
                    }
                }
                if (eq.comparsionAtom == math_parser.atom.ComparsionSymbolAtom.Eq)
                {
                    if (lhs_literal > 0) // 1 + 3x = 0 -> -1 - 3x = 0 -> -3x = 1 -> - 3x <= 1 (no a), -3x >= 1 (with a)
                    {
                        construct_meta.Add(new Meta(null, used_slack, new EqResult(-eq.exprs, math_parser.atom.ComparsionSymbolAtom.Le)));
                        used_slack++;
                        construct_meta.Add(new Meta(used_artifical, used_slack, new EqResult(-eq.exprs, math_parser.atom.ComparsionSymbolAtom.Ge)));
                        used_artifical++;
                        used_slack++;
                    }
                    else
                    { // -1 + 3x = 0 -> 3x = 1 -> 3x <= 1 (no a), 3x >= 1 (with a)
                      // 0 + 3x = 0 -> 3x = 0 -> 3x <= 0 (no a), 3x >= 0 (with a)
                        construct_meta.Add(new Meta(null, used_slack, new EqResult(eq.exprs, math_parser.atom.ComparsionSymbolAtom.Le)));
                        used_slack++;
                        construct_meta.Add(new Meta(used_artifical, used_slack, new EqResult(eq.exprs, math_parser.atom.ComparsionSymbolAtom.Ge)));
                        used_artifical++;
                        used_slack++;
                    }
                }
                else if (eq.comparsionAtom == math_parser.atom.ComparsionSymbolAtom.Le)
                {
                    if (lhs_literal > 0) // 1 + 3x <= 0 -> -1 - 3x >= 0 3x >= 1 (with a)
                    {
                        construct_meta.Add(new Meta(used_artifical, used_slack, new EqResult(-eq.exprs, math_parser.atom.ComparsionSymbolAtom.Ge)));
                        used_artifical++;
                        used_slack++;
                    }
                    else
                    { // -1 + 3x <= 0 -> 3x <= 1
                      // 0 + 3x <= 0 -> 3x <= 0
                        construct_meta.Add(new Meta(null, used_slack, new EqResult(eq.exprs, math_parser.atom.ComparsionSymbolAtom.Le)));
                        used_slack++;
                    }
                }
                else if (eq.comparsionAtom == math_parser.atom.ComparsionSymbolAtom.Ge)
                {
                    if (lhs_literal >= 0) // 1 + 3x >= 0 -> -1 - 3x <= 0 -> -3x <= 1 (no a)
                    // 0 + 3x >= 0 -> -3x <= 0 (no a)
                    {
                        construct_meta.Add(new Meta(null, used_slack, new EqResult(-eq.exprs, math_parser.atom.ComparsionSymbolAtom.Le)));
                        used_slack++;
                    }
                    else
                    { // -1 + 3x >= 0 -> 3x >= 1 (with a)
                        construct_meta.Add(new Meta(used_artifical, used_slack, new EqResult(eq.exprs, math_parser.atom.ComparsionSymbolAtom.Ge)));
                        used_artifical++;
                        used_slack++;
                    }
                }
            }

            // 2x + y <= 120
            // 2x + y -120 <= 0
            // 2x + y -s2 + a1 - 120 = 0
            // a1 = 120 - 2x - y + s2
            // Obj: MIN A - a1
            // A - (120 - 2x - y + s2)
            // A +2x + y -s2 - 120

            // + 2: P, RHS, conditional: A // + 1: objective, conditional: 2 stage objective
            Fraction[,] values = new Fraction[varNames.Count + used_artifical + used_slack + 2  + (used_artifical > 0 ? 1 : 0), construct_meta.Count + 1 + (used_artifical > 0 ? 1 : 0)];
            // Initialize to 0
            for (int _i = 0; _i < values.GetLength(0); _i++)
            {
                for (int _j = 0; _j < values.GetLength(1); _j++)
                {
                    values[_i, _j] = 0;
                }
            }
            int padding_start = used_artifical > 0 ? 2 : 1;
            if (used_artifical > 0)
            {
                values[0, 0] = 1;
                values[1, 0] = 0;
                foreach (Meta m in construct_meta) // A row
                {
                    if (m.artifical is null) continue;
                    foreach (Term t in m.result.exprs.terms)
                    {
                        if (t.term_name == "")
                        {
                            values[values.GetLength(0) - 1, 0] -= t.coefficient.Transitivity(); // -(-120) on RHS so getting 120
                        }
                        else
                        {
                            int var_idx = varNames.IndexOf(t.term_name);
                            if (var_idx >= 0)
                            {
                                values[var_idx + padding_start, 0] += t.coefficient.Transitivity();
                            }
                        }
                    }
                    values[padding_start + varNames.Count + m.slack, 0] -= 1;
                }
            }
            values[padding_start - 1, padding_start - 1] = 1; // P variable
            foreach (Term t in optimal.terms) // P row
            {
                if (t.term_name == "") continue;
                int var_idx = varNames.IndexOf(t.term_name);
                if (var_idx >= 0)
                {
                    values[var_idx + padding_start, padding_start - 1] = -t.coefficient.Transitivity();
                    // P = 1x + 3y
                    // P - 1x - 3y = 0
                }
            }
            for (int i = 0; i < construct_meta.Count; i++)
            {
                Meta m = construct_meta[i];
                foreach (Term t in m.result.exprs.terms)
                {
                    if (t.term_name == "")
                    {
                        values[values.GetLength(0) - 1, i + padding_start] += -t.coefficient.Transitivity(); // -(-120) on RHS so getting 120
                    }
                    else
                    {
                        int var_idx = varNames.IndexOf(t.term_name);
                        if (var_idx >= 0)
                        {
                            values[var_idx + padding_start, i + padding_start] += t.coefficient.Transitivity();
                        }
                    }
                }
                if (m.artifical is null)
                {
                    values[varNames.Count + padding_start + m.slack, i + padding_start] = 1;
                }
                else
                {
                    values[varNames.Count + padding_start + m.slack, i + padding_start] = -1;
                    values[varNames.Count + padding_start + used_slack + m.artifical.Value, i + padding_start] = 1;
                }
            }

            List<string> full_vars = new List<string>();
            if (used_artifical > 0)
            {
                full_vars.Add("A");
            }
            full_vars.Add("P");
            foreach (string v in varNames)
            {
                full_vars.Add(v);
            }
            for (int i = 0; i < used_slack; i++)
            {
                full_vars.Add($"s_{i + 1}");
            }
            for (int i = 0; i < used_artifical; i++)
            {
                full_vars.Add($"a_{i + 1}");
            }

            return new SimplexInterationRunner(
                used_artifical > 0 ? SimplexStage.TWO_STAGE_MAX : SimplexStage.ONE_STAGE,
                used_artifical > 0 ? SimplexMode.MIN : SimplexMode.MAX,
                values,
                full_vars.ToArray()
            );
        }

        public static SimplexInterationRunner Translate(SimplexInputTable table)
        {
            List<string> varNames = new List<string>();
            bool has_artifical = false;
            for (int x = 0; x < table.GetSize().x; x++)
            {
                string varName = ((SimplexTableauVariableField)table[x, 0]).content.Trim();
                if (!IsValidVariableName(varName)) throw new SimplexError($"Invalid variable name: {varName}");
                if (varNames.Contains(varName)) throw new SimplexError($"Duplicate variable name: {varName}");
                if (IsArtificialVariable(varName)) has_artifical = true;
                varNames.Add(varName);
            }
            Fraction[,] values = new Fraction[table.GetSize().x + 1, table.GetSize().y];
            // Initialize array entries to zero fraction to avoid default-denominator==0
            for (int _i = 0; _i < values.GetLength(0); _i++)
            {
                for (int _j = 0; _j < values.GetLength(1); _j++)
                {
                    values[_i, _j] = (Fraction)0;
                }
            }
            for (int x = 0; x <= table.GetSize().x; x++)
            {
                for (int y = 1; y < table.GetSize().y + 1; y++)
                {
                    string cellContent = ((SimplexTableauValueField)table[x, y]).content.Trim();
                    if (!Fraction.TryParse(cellContent, out Fraction v)) throw new SimplexError($"Invalid fraction value at cell ({x + Const.DISPLAY_INDEX_OFFSET}, {y}): {cellContent + Const.DISPLAY_INDEX_OFFSET}");
                    values[x, y - 1] = v;
                }
            }
            return new SimplexInterationRunner(
                has_artifical ? SimplexStage.TWO_STAGE_MAX : SimplexStage.ONE_STAGE,
                has_artifical ? SimplexMode.MIN : SimplexMode.MAX,
                values,
                varNames.ToArray()
            );
        }

        public static SimplexRunnerOutput[] RunAll(SimplexInterationRunner runner)
        {
            if (runner is null)
            {
                ui.DEBUG.DebugStore.AppendLine("Runner is null, cannot run.");
                return new SimplexRunnerOutput[] { };
            }
            SimplexInterationRunner current = (SimplexInterationRunner)runner;
            int idx = 0;
            List<SimplexRunnerOutput> results = new List<SimplexRunnerOutput>
            {
                new SimplexRunnerOutput(SimplexState.NOT_ENDED, current, "Initial state")
            };
            while (true)
            {
                ui.DEBUG.DebugStore.AppendLine($"--- Step {idx} ---");
                ui.DEBUG.DebugStore.AppendLine(current.ToString());
                SimplexRunnerOutput output = current.Next();
                ui.DEBUG.DebugStore.AppendLine($"State: {output.state}, Reason: {output.reason}");
                if (!(output.next is null)) results.Add(output);
                if (output.state != SimplexState.NOT_ENDED)
                {
                    if (output.state == SimplexState.FAILED) throw new SimplexError("Simplex method failed to complete.");
                    else if (output.state == SimplexState.ENDED) ui.DEBUG.DebugStore.AppendLine("Simplex method completed successfully.");
                    break;
                }
                current = (SimplexInterationRunner)output.next;
                idx++;
            }
            return results.ToArray();
        }
    }
}