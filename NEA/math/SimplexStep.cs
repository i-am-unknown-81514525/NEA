using ui.math;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NEA.math
{
    public struct SimplexStep
    {
        public SimplexStage stage;
        public SimplexMode mode;
        public Fraction[,] expressions;
        public string[] vars;

        public (SimplexState state, SimplexStep? next, string info) Next()
        // Return same step if ended
        {
            var expressions = this.expressions;
            var vars = this.vars;
            string info = "";
            if (stage == SimplexStage.SOLVED)
            {
                return (SimplexState.ENDED, this, info + "\nThe problem is solved");
            }
            if (isCompleted())
            {
                SimplexStep step = (SimplexStep)MemberwiseClone();
                step.stage = SimplexStage.SOLVED;
                return (SimplexState.ENDED, step, info + "\nAlready completed");
            }
            int order = 0;
            int col = 0;
            int row = 0;
            while (true)
            {
                int? ncol = GetColIdxByMode(this.mode, order);
                if (ncol is null)
                {
                    return (SimplexState.FAILED, null, info + "\nCannot select a valid pivotal column");
                }
                col = (int)ncol;
                int? nrow = GetRowIdxByMode(col, this.mode);
                if (nrow is null)
                {
                    order++;
                    continue;
                }
                row = (int)nrow;
                break;
            }
            Fraction pivot_value = expressions[col, row];
            Fraction[] expr = Enumerable.Range(0, expressions.GetLength(0))
                .Select(x => expressions[x, row] / pivot_value).ToArray();
            Fraction[,] new_expressions = new Fraction[expressions.GetLength(0), expressions.GetLength(1)];
            for (int y = 0; y < expressions.GetLength(1); y++)
            {
                if (y == row)
                {
                    for (int x = 0; x < expressions.GetLength(0); x++)
                    {
                        new_expressions[x, y] = expr[x];
                    }
                    continue;
                }
                Fraction amount = -expressions[col, y];
                for (int x = 0; x < expressions.GetLength(0); x++)
                {
                    new_expressions[x, y] = expressions[x, y] + (amount * expr[x]);
                }
            }
            if (stage == SimplexStage.TWO_STAGE_MIN || stage == SimplexStage.TWO_STAGE_MAX)
            {
                bool isCompleted = true;
                List<int> artificalIdx = new List<int>();
                for (int x = 0; x < expressions.GetLength(0) - 1; x++)
                {
                    string var_name = vars[x];
                    Fraction curr_value = new_expressions[x, 0];
                    if (var_name == "A")
                    {
                        artificalIdx.Add(x);
                        if (curr_value != 1)
                        {
                            return (SimplexState.FAILED, null, info + "\nVar A on row 0 isn't 1");
                        }
                    }
                    else if (var_name.StartsWith("a_") && var_name.Length > 2 && int.TryParse(var_name.Substring(2), out _))
                    {
                        artificalIdx.Add(x);
                        if (curr_value != -1)
                        {
                            isCompleted = false;
                            break;
                        }
                    }
                    else
                    {
                        if (curr_value != 0)
                        {
                            isCompleted = false;
                            break;
                        }
                    }
                }
                if (isCompleted)
                {
                    int[] notArtifical = Enumerable.Range(0, expressions.GetLength(0)).Where(idx => !artificalIdx.Contains(idx)).ToArray();
                    Fraction[,] notArtificalExpr = new Fraction[notArtifical.Length, expressions.GetLength(1) - 1];
                    for (int x = 0; x < notArtifical.Length; x++)
                    {
                        for (int y = 0; y < expressions.GetLength(1) - 1; y++)
                        {
                            notArtificalExpr[x, y] = new_expressions[notArtifical[x], y + 1];
                        }
                    }
                    SimplexStep new_step_art = new SimplexStep();
                    new_step_art.vars = notArtifical.Take(notArtifical.Length - 1).Select(idx => vars[idx]).ToArray(); // Exclude the last one which is RHS
                    new_step_art.stage = SimplexStage.ONE_STAGE;
                    new_step_art.expressions = notArtificalExpr;
                    new_step_art.mode = stage == SimplexStage.TWO_STAGE_MIN ? SimplexMode.MIN : SimplexMode.MAX;
                    return (SimplexState.NOT_ENDED, new_step_art, info + "\nRemove artifical variables");
                }
            }
            SimplexStep new_step = new SimplexStep();
            new_step.vars = vars;
            new_step.stage = stage;
            new_step.expressions = new_expressions;
            new_step.mode = mode;
            SimplexState state = SimplexState.NOT_ENDED;
            if (isCompleted())
            {
                state = SimplexState.ENDED;
                new_step.stage = SimplexStage.SOLVED;
            }
            return (state, new_step, info);
        }

        private int? GetColIdxByMode(SimplexMode mode, int order = 0)
        {
            var expressions = this.expressions;
            var selections = Enumerable.Range(0, expressions.GetLength(0) - 1)
                .Select(x => (x, expressions[x, 0]))
                .Select(((int i, Fraction frac) item) => mode == SimplexMode.MAX ? item : (item.i, -item.frac))
                .Where(item => item.Item2 < 0)
                .OrderBy(idx_frac => idx_frac.Item2)
                .ToArray();
            if (selections.Length <= order)
                return null;
            return selections[order].Item1;
        }

        private int? GetRowIdxByMode(int col, SimplexMode mode)
        {
            var expressions = this.expressions;
            var selections = Enumerable.Range(1, expressions.GetLength(1) - 1)
                .Select(y => (y, expressions[col, y]))
                .Select(item => mode == SimplexMode.MAX ? item : (item.y, -item.Item2))
                .Where(item => item.Item2 > 0 && expressions[expressions.GetLength(0) - 1, item.y] / item.Item2 > 0)
                .Select(item => (item.y, expressions[expressions.GetLength(0) - 1, item.y] / item.Item2))
                .OrderBy(idx_frac => idx_frac.Item2)
                .ToArray();
            if (selections.Length == 0)
                return null;
            return selections[0].y;
        }

        private bool isCompleted()
        {
            var expressions = this.expressions;
            var mode = this.mode;
            return vars[0] == "P" && Enumerable.Range(0, expressions.GetLength(0)).Select(x => expressions[x, 0]).All(value => mode == SimplexMode.MAX ? value >= 0 : value <= 0);
        }

        public Dictionary<string, Fraction> Resolve()
        {
            Dictionary<string, Fraction> resolved = new Dictionary<string, Fraction>();
            var expressions = this.expressions;
            if (!isCompleted())
            {
                throw new InvalidOperationException();
            }
            List<int> selectedRowIdx = new List<int>();

            for (int x = 0; x < expressions.GetLength(0) - 1; x++) // For each column
            {
                int[] count1 = Enumerable.Range(0, expressions.GetLength(1)).Where(y => expressions[x, y] == 1).ToArray(); // Check each value on the column
                int[] other = Enumerable.Range(0, expressions.GetLength(1)).Where(y => expressions[x, y] != 0 && expressions[x, y] != 1).ToArray();
                if (other.Count() == 0 && count1.Count() == 1)
                {
                    int idx = count1[0];
                    if (selectedRowIdx.Contains(idx))
                    {
                        continue;
                    }
                    selectedRowIdx.Add(idx);
                    resolved[vars[x]] = expressions[expressions.GetLength(0) - 1, idx];
                }
            }
            return resolved;
        }
    }
}
