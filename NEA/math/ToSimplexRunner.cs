using NEA.components;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ui.math;

// ^[a-zA-Z][a-zA-Z0-9]*(?:_[a-zA-Z0-9]+)?$

namespace NEA.math
{
    public static class ToSimplexRunner
    {

        public static bool IsArtificialVariable(string name)
        {
            return IsValidVariableName(name) && name.StartsWith("a_");
        }

        public static bool IsValidVariableName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9]*(?:_[a-zA-Z0-9]+)?$");
        }

        public static SimplexInterationRunner? Translate(SimplexInputTable table)
        {
            List<string> varNames = new List<string>();
            bool has_artifical = false;
            for (int x = 0; x < table.GetSize().x; x++)
            {
                string varName = ((SimplexTableauVariableField)table[x, 0]).content.Trim();
                if (!IsValidVariableName(varName)) return null;
                if (varNames.Contains(varName)) return null;
                if (IsArtificialVariable(varName)) has_artifical = true;
                varNames.Add(varName);
            }
            Fraction[,] values = new Fraction[table.GetSize().x + 1, table.GetSize().y];
            for (int x = 0; x <= table.GetSize().x; x++)
            {
                for (int y = 1; y < table.GetSize().y + 1; y++)
                {
                    string cellContent = ((SimplexTableauValueField)table[x, y]).content.Trim();
                    if (!Fraction.TryParse(cellContent, out Fraction v)) return null;
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

        public static void RunAll(SimplexInterationRunner? runner)
        {
            if (runner is null)
            {
                ui.DEBUG.DebugStore.AppendLine("Runner is null, cannot run.");
                return;
            }
            SimplexInterationRunner current = (SimplexInterationRunner)runner;
            int idx = 0;
            while (true)
            {
                ui.DEBUG.DebugStore.AppendLine($"--- Step {idx} ---");
                ui.DEBUG.DebugStore.AppendLine(current.ToString());
                (SimplexState state, SimplexInterationRunner? next, string reason) = current.Next();
                ui.DEBUG.DebugStore.AppendLine($"State: {state}, Reason: {reason}");
                if (state != SimplexState.NOT_ENDED)
                {
                    if (state == SimplexState.FAILED) ui.DEBUG.DebugStore.AppendLine("Simplex method failed to complete.");
                    else if (state == SimplexState.ENDED) ui.DEBUG.DebugStore.AppendLine("Simplex method completed successfully.");
                    break;
                }
                current = (SimplexInterationRunner)next;
                idx++;
            }
        }
    }
}