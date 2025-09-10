using System.Linq;
using ui.math;
using System;
using System.Collections.Generic;

namespace NEA.math
{
    public struct SimplexInterationRunner
    {
        public SimplexStage stage;
        public SimplexMode mode;

        public SimplexStep step;
        public Fraction[,] expressions;
        public string[] vars;
        public (int pivotCol, int pivotRow, Fraction[] normalised, List<int> artificalIdx) meta;

        public SimplexInterationRunner Clone()
        {
            return new SimplexInterationRunner
            {
                stage = stage,
                mode = mode,
                step = step,
                expressions = (Fraction[,])expressions.Clone(),
                vars = (string[])vars.Clone(),
                meta = (meta.pivotCol, meta.pivotRow, meta.normalised is null ? null : (Fraction[])meta.normalised.Clone(), meta.artificalIdx.ToList())
            };
        }

        public (SimplexState state, SimplexInterationRunner? next, string reason) Next()
        {
            SimplexInterationRunner new_runner = Clone();
            switch (step)
            {
                case SimplexStep.PICK_PIVOT_COLUMN:
                    {
                        int? result = GetColIdxByMode(mode);
                        string pos = mode == SimplexMode.MAX ? "negative" : "positive";
                        if (result is null)
                        {
                            if (isCompleted())
                                return (SimplexState.ENDED, this, $"There are no {pos} (unoptimal) column in objective function, and therefore a solution is found");
                            else
                                return (SimplexState.FAILED, null, $"Contradictory result: No pivot column can be found but the solution isn't complete");
                        }
                        new_runner.step = SimplexStep.PICK_PIVOT_ROW;
                        new_runner.meta.pivotCol = (int)result;
                        return (SimplexState.NOT_ENDED, new_runner, $"Picked pivot column with index of: {result + Const.DISPLAY_INDEX_OFFSET} as this is not optimal solution");
                    }
                case SimplexStep.PICK_PIVOT_ROW:
                    {
                        int? result = GetRowIdxByMode(meta.pivotCol, mode);
                        if (result is null)
                        {
                            return (SimplexState.FAILED, null, $"Cannot select a pivot row where RHS/col > 0 with pivot column with index: {meta.pivotCol + Const.DISPLAY_INDEX_OFFSET}");
                        }
                        new_runner.step = SimplexStep.NORMALISE_ROW;
                        new_runner.meta.pivotRow = (int)result;
                        return (SimplexState.NOT_ENDED, new_runner, $"Picked pivot row with index of: {result + Const.DISPLAY_INDEX_OFFSET}");
                    }
                case SimplexStep.NORMALISE_ROW:
                    {
                        Fraction value = expressions[meta.pivotCol, meta.pivotRow];
                        Fraction[] normalised = new Fraction[expressions.GetLength(0)];
                        for (int x = 0; x < expressions.GetLength(0); x++)
                        {
                            if (Const.SIMPLEX_NORMALISE_INPACE_MOD)
                                new_runner.expressions[x, meta.pivotRow] = expressions[x, meta.pivotRow] / value;
                            normalised[x] = expressions[x, meta.pivotRow] / value;
                        }
                        new_runner.step = SimplexStep.APPLY_OTHER;
                        new_runner.meta.normalised = normalised;
                        return (SimplexState.NOT_ENDED, new_runner, $"Normalise pivot row {meta.pivotRow} by multiply the row with {1 / value}");
                    }
                case SimplexStep.APPLY_OTHER:
                    {
                        if (!Const.SIMPLEX_NORMALISE_INPACE_MOD)
                        {
#pragma warning disable CS0162
                            for (int x = 0; x < expressions.GetLength(0); x++)
                            {
                                new_runner.expressions[x, meta.pivotRow] = meta.normalised[x];
                            }
#pragma warning restore CS0162
                        }
                        for (int y = 0; y < expressions.GetLength(1); y++)
                        {
                            if (y == meta.pivotRow) continue;
                            Fraction value = -expressions[meta.pivotCol, y];
                            for (int x = 0; x < expressions.GetLength(0); x++)
                            {
                                new_runner.expressions[x, y] = expressions[x, y] + value * meta.normalised[x];
                            }
                        }
                        if (stage == SimplexStage.TWO_STAGE_MIN || stage == SimplexStage.TWO_STAGE_MAX)
                        {
                            new_runner.step = SimplexStep.CHECK_ARTIFICAL;
                        }
                        else
                        {
                            new_runner.step = SimplexStep.PICK_PIVOT_COLUMN;
                        }
                        return (SimplexState.NOT_ENDED, new_runner, "For each non-pivot row, put the new row as old_row + (-pivot_value)*pivot_row");
                    }
                case SimplexStep.CHECK_ARTIFICAL:
                    {
                        bool isCompleted = true;
                        List<int> artificalIdx = new List<int>();
                        for (int x = 0; x < expressions.GetLength(0) - 1; x++)
                        {
                            string var_name = vars[x];
                            Fraction curr_value = expressions[x, 0];
                            if (var_name == "A")
                            {
                                artificalIdx.Add(x);
                                if (curr_value != 1)
                                {
                                    return (SimplexState.FAILED, null, "Var A on row 0 isn't 1");
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
                        string reason;
                        if (isCompleted)
                        {
                            new_runner.step = SimplexStep.REMOVE_ARTIFICAL;
                            reason = "A = 0 => The tableau can transition to Stage 2 Simplex";
                        }
                        else
                        {
                            new_runner.step = SimplexStep.PICK_PIVOT_COLUMN;
                            reason = "A != 0";
                        }
                        new_runner.meta.artificalIdx = artificalIdx;
                        return (SimplexState.NOT_ENDED, new_runner, reason);
                    }
                case SimplexStep.REMOVE_ARTIFICAL:
                    {
                        var artificalIdx = new_runner.meta.artificalIdx;
                        int[] notArtifical = Enumerable.Range(0, expressions.GetLength(0)).Where(idx => !artificalIdx.Contains(idx)).ToArray();
                        Fraction[,] notArtificalExpr = new Fraction[notArtifical.Length, expressions.GetLength(1) - 1];
                        for (int x = 0; x < notArtifical.Length; x++)
                        {
                            for (int y = 0; y < expressions.GetLength(1) - 1; y++)
                            {
                                notArtificalExpr[x, y] = expressions[notArtifical[x], y + 1];
                            }
                        }
                        var vars = this.vars;
                        new_runner.vars = notArtifical.Take(notArtifical.Length - 1).Select(idx => vars[idx]).ToArray(); // Exclude the last one which is RHS
                        new_runner.stage = SimplexStage.ONE_STAGE;
                        new_runner.expressions = notArtificalExpr;
                        new_runner.mode = stage == SimplexStage.TWO_STAGE_MIN ? SimplexMode.MIN : SimplexMode.MAX;
                        new_runner.step = SimplexStep.PICK_PIVOT_COLUMN;
                        new_runner.step = SimplexStep.PICK_PIVOT_COLUMN;
                        return (SimplexState.NOT_ENDED, new_runner, "Remove artifical variables");
                    }
                default:
                    {
                        return (SimplexState.FAILED, null, "Stuck at undefined state");
                    }
            }
        }

        private int? GetColIdxByMode(SimplexMode mode)
        {
            var expressions = this.expressions;
            var selection = Enumerable.Range(0, expressions.GetLength(0) - 1)
                .Select(x => (x, expressions[x, 0]))
                .Select(((int i, Fraction frac) item) => mode == SimplexMode.MAX ? item : (item.i, -item.frac))
                .Where(item => item.Item2 < 0)
                .OrderBy(idx_frac => idx_frac.Item2)
                .ToArray();
            if (selection.Length == 0)
            {
                return null;
            }
            return selection[0].Item1;
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
