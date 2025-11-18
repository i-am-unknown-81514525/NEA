using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using math_parser.tokenizer;
using NEA.components;
using NEA.Math;
using ui;
using ui.math;
using ComparsionSymbolAtom = math_parser.atom.ComparsionSymbolAtom;

// ^[a-zA-Z][a-zA-Z0-9]*(?:_[a-zA-Z0-9]+)?$

namespace NEA.math
{
    struct Meta
    {
        public readonly int? Artifical;
        public readonly int Slack;
        public readonly EqResult Result;

        public Meta(int? artifical, int slack, EqResult result)
        {
            Artifical = artifical;
            Slack = slack;
            Result = result;
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

            if (!stream.isEof)
            {
                throw new SimplexError("Not End of line when parsing final token");
            }


            ExprResult objective = (ExprResult)result.ParseResult[2];

            RepeatListResult<TokenSequenceResult<ParseResult>> eqsToken = (RepeatListResult<TokenSequenceResult<ParseResult>>)result.ParseResult[6];

            List<EqResult> eqs = new List<EqResult>();

            foreach (TokenSequenceResult<ParseResult> res in eqsToken)
            {
                eqs.Add((EqResult)res.ParseResult[0]);
            }
            return Translate(objective, eqs.ToArray());
        }

        public static SimplexInterationRunner Translate((ExprResult optimal, EqResult[] constraints) inputs) => Translate(inputs.optimal, inputs.constraints);

        public static SimplexInterationRunner Translate(ExprResult optimal, EqResult[] constraints)
        {
            List<string> varNames = new List<string>();
            int usedArtifical = 0;
            int usedSlack = 0;
            foreach (Term term in optimal.Terms)
            {
                if (term.TermName == "") continue;
                if (!IsValidVariableName(term.TermName)) {
                    throw new SimplexError($"Invalid variable name: {term.TermName}");
                }
                if (IsArtificialVariable(term.TermName) || IsSlackVariable(term.TermName)) {
                    throw new SimplexError($"Invalid variable name: {term.TermName}");
                }
                if (varNames.Contains(term.TermName)) {
                    throw new SimplexError($"Duplicate variable name: {term.TermName}");
                }
                varNames.Add(term.TermName);
            }

            List<Meta> constructMeta = new List<Meta>();
            foreach (EqResult eq in constraints)
            {
                Fraction lhsLiteral = 0; // LHS, which is opposite to RHS
                foreach (Term t in eq.Exprs.Terms)
                {
                    if (t.TermName == "")
                    {
                        lhsLiteral += t.Coefficient.Transitivity();
                    } else
                    {
                        if (!IsValidVariableName(t.TermName)) {
                            throw new SimplexError($"Invalid variable name: {t.TermName}");
                        }
                        if (IsArtificialVariable(t.TermName) || IsSlackVariable(t.TermName)) {
                            throw new SimplexError($"Invalid variable name: {t.TermName}");
                        }
                        if (!varNames.Contains(t.TermName))
                        {
                            varNames.Add(t.TermName);
                        }
                    }
                }
                if (eq.ComparsionAtom == ComparsionSymbolAtom.Eq)
                {
                    if (lhsLiteral > 0) // 1 + 3x = 0 -> -1 - 3x = 0 -> -3x = 1 -> - 3x <= 1 (no a), -3x >= 1 (with a)
                    {
                        constructMeta.Add(new Meta(null, usedSlack, new EqResult(-eq.Exprs, ComparsionSymbolAtom.Le)));
                        usedSlack++;
                        constructMeta.Add(new Meta(usedArtifical, usedSlack, new EqResult(-eq.Exprs, ComparsionSymbolAtom.Ge)));
                        usedArtifical++;
                        usedSlack++;
                    }
                    else
                    { // -1 + 3x = 0 -> 3x = 1 -> 3x <= 1 (no a), 3x >= 1 (with a)
                      // 0 + 3x = 0 -> 3x = 0 -> 3x <= 0 (no a), 3x >= 0 (with a)
                        constructMeta.Add(new Meta(null, usedSlack, new EqResult(eq.Exprs, ComparsionSymbolAtom.Le)));
                        usedSlack++;
                        constructMeta.Add(new Meta(usedArtifical, usedSlack, new EqResult(eq.Exprs, ComparsionSymbolAtom.Ge)));
                        usedArtifical++;
                        usedSlack++;
                    }
                }
                else if (eq.ComparsionAtom == ComparsionSymbolAtom.Le)
                {
                    if (lhsLiteral > 0) // 1 + 3x <= 0 -> -1 - 3x >= 0 3x >= 1 (with a)
                    {
                        constructMeta.Add(new Meta(usedArtifical, usedSlack, new EqResult(-eq.Exprs, ComparsionSymbolAtom.Ge)));
                        usedArtifical++;
                        usedSlack++;
                    }
                    else
                    { // -1 + 3x <= 0 -> 3x <= 1
                      // 0 + 3x <= 0 -> 3x <= 0
                        constructMeta.Add(new Meta(null, usedSlack, new EqResult(eq.Exprs, ComparsionSymbolAtom.Le)));
                        usedSlack++;
                    }
                }
                else if (eq.ComparsionAtom == ComparsionSymbolAtom.Ge)
                {
                    if (lhsLiteral >= 0) // 1 + 3x >= 0 -> -1 - 3x <= 0 -> -3x <= 1 (no a)
                    // 0 + 3x >= 0 -> -3x <= 0 (no a)
                    {
                        constructMeta.Add(new Meta(null, usedSlack, new EqResult(-eq.Exprs, ComparsionSymbolAtom.Le)));
                        usedSlack++;
                    }
                    else
                    { // -1 + 3x >= 0 -> 3x >= 1 (with a)
                        constructMeta.Add(new Meta(usedArtifical, usedSlack, new EqResult(eq.Exprs, ComparsionSymbolAtom.Ge)));
                        usedArtifical++;
                        usedSlack++;
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
            Fraction[,] values = new Fraction[varNames.Count + usedArtifical + usedSlack + 2  + (usedArtifical > 0 ? 1 : 0), constructMeta.Count + 1 + (usedArtifical > 0 ? 1 : 0)];
            // Initialize to 0
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    values[i, j] = 0;
                }
            }
            int paddingStart = usedArtifical > 0 ? 2 : 1;
            if (usedArtifical > 0)
            {
                values[0, 0] = 1;
                values[1, 0] = 0;
                foreach (Meta m in constructMeta) // A row
                {
                    if (m.Artifical is null) continue;
                    foreach (Term t in m.Result.Exprs.Terms)
                    {
                        if (t.TermName == "")
                        {
                            values[values.GetLength(0) - 1, 0] -= t.Coefficient.Transitivity(); // -(-120) on RHS so getting 120
                        }
                        else
                        {
                            int varIdx = varNames.IndexOf(t.TermName);
                            if (varIdx >= 0)
                            {
                                values[varIdx + paddingStart, 0] += t.Coefficient.Transitivity();
                            }
                        }
                    }
                    values[paddingStart + varNames.Count + m.Slack, 0] -= 1;
                }
            }
            values[paddingStart - 1, paddingStart - 1] = 1; // P variable
            foreach (Term t in optimal.Terms) // P row
            {
                if (t.TermName == "") continue;
                int varIdx = varNames.IndexOf(t.TermName);
                if (varIdx >= 0)
                {
                    values[varIdx + paddingStart, paddingStart - 1] = -t.Coefficient.Transitivity();
                    // P = 1x + 3y
                    // P - 1x - 3y = 0
                }
            }
            for (int i = 0; i < constructMeta.Count; i++)
            {
                Meta m = constructMeta[i];
                foreach (Term t in m.Result.Exprs.Terms)
                {
                    if (t.TermName == "")
                    {
                        values[values.GetLength(0) - 1, i + paddingStart] += -t.Coefficient.Transitivity(); // -(-120) on RHS so getting 120
                    }
                    else
                    {
                        int varIdx = varNames.IndexOf(t.TermName);
                        if (varIdx >= 0)
                        {
                            values[varIdx + paddingStart, i + paddingStart] += t.Coefficient.Transitivity();
                        }
                    }
                }
                if (m.Artifical is null)
                {
                    values[varNames.Count + paddingStart + m.Slack, i + paddingStart] = 1;
                }
                else
                {
                    values[varNames.Count + paddingStart + m.Slack, i + paddingStart] = -1;
                    values[varNames.Count + paddingStart + usedSlack + m.Artifical.Value, i + paddingStart] = 1;
                }
            }

            List<string> fullVars = new List<string>();
            if (usedArtifical > 0)
            {
                fullVars.Add("A");
            }
            fullVars.Add("P");
            foreach (string v in varNames)
            {
                fullVars.Add(v);
            }
            for (int i = 0; i < usedSlack; i++)
            {
                fullVars.Add($"s_{i + 1}");
            }
            for (int i = 0; i < usedArtifical; i++)
            {
                fullVars.Add($"a_{i + 1}");
            }

            return new SimplexInterationRunner(
                usedArtifical > 0 ? SimplexStage.TWO_STAGE_MAX : SimplexStage.ONE_STAGE,
                usedArtifical > 0 ? SimplexMode.MIN : SimplexMode.MAX,
                values,
                fullVars.ToArray()
            );
        }

        public static SimplexInterationRunner Translate(SimplexInputTable table)
        {
            List<string> varNames = new List<string>();
            bool hasArtifical = false;
            for (int x = 0; x < table.GetSize().x; x++)
            {
                string varName = ((SimplexTableauVariableField)table[x, 0]).content.Trim();
                if (!IsValidVariableName(varName)) throw new SimplexError($"Invalid variable name: {varName}");
                if (varNames.Contains(varName)) throw new SimplexError($"Duplicate variable name: {varName}");
                if (IsArtificialVariable(varName)) hasArtifical = true;
                varNames.Add(varName);
            }
            Fraction[,] values = new Fraction[table.GetSize().x + 1, table.GetSize().y];
            // Initialize array entries to zero fraction to avoid default-denominator==0
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    values[i, j] = (Fraction)0;
                }
            }
            for (int x = 0; x <= table.GetSize().x; x++)
            {
                for (int y = 1; y < table.GetSize().y + 1; y++)
                {
                    string cellContent = ((SimplexTableauValueField)table[x, y]).content.Trim();
                    if (cellContent == "") cellContent = "0";
                    if (!Fraction.TryParse(cellContent, out Fraction v)) throw new SimplexError($"Invalid fraction value at cell ({x + Const.DisplayIndexOffset}, {y + Const.DisplayIndexOffset}): {cellContent}");
                    values[x, y - 1] = v;
                }
            }
            return new SimplexInterationRunner(
                hasArtifical ? SimplexStage.TWO_STAGE_MAX : SimplexStage.ONE_STAGE,
                hasArtifical ? SimplexMode.MIN : SimplexMode.MAX,
                values,
                varNames.ToArray()
            );
        }

        public static SimplexRunnerOutput[] RunAll(SimplexInterationRunner runner)
        {
            if (runner is null)
            {
                Debug.DebugStore.AppendLine("Runner is null, cannot run.");
                return new SimplexRunnerOutput[] { };
            }
            SimplexInterationRunner current = runner;
            int idx = 0;
            List<SimplexRunnerOutput> results = new List<SimplexRunnerOutput>
            {
                new SimplexRunnerOutput(SimplexState.NOT_ENDED, current, "Initial state")
            };
            while (true)
            {
                Debug.DebugStore.AppendLine($"--- Step {idx} ---");
                Debug.DebugStore.AppendLine(current.ToString());
                SimplexRunnerOutput output = current.Next();
                Debug.DebugStore.AppendLine($"State: {output.State}, Reason: {output.Reason}");
                if (!(output.Next is null)) results.Add(output);
                if (output.State != SimplexState.NOT_ENDED)
                {
                    if (output.State == SimplexState.FAILED) throw new SimplexError("Simplex method failed to complete.");
                    if (output.State == SimplexState.ENDED) Debug.DebugStore.AppendLine("Simplex method completed successfully.");
                    break;
                }
                current = output.Next;
                idx++;
            }
            return results.ToArray();
        }
    }
}