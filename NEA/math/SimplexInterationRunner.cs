using System.Linq;
using ui.math;
using System;
using System.Collections.Generic;
using NEA.components;
using ui.LatexExt;

namespace NEA.math
{
    public readonly struct SimplexRunnerOutput : ILatex
    {
        public readonly SimplexState State;
        public readonly SimplexInterationRunner Next;
        public readonly string Reason;

        public SimplexRunnerOutput(SimplexState state, SimplexInterationRunner next, string reason)
        {
            this.State = state;
            this.Next = next;
            this.Reason = reason;
        }

        public SimplexOutputContainer ToOutputContainer()
        {
            return new SimplexOutputContainer(State, (SimplexInterationRunner)Next, Reason);
        }

        public string AsLatex()
        {
            return $"\\\\\\text{{{Reason.Replace("_", "\\_")}}}\\\\\n{ToOutputContainer().AsLatex()}";
        }
    }
    public class SimplexInterationRunner
    {
        public SimplexStage Stage;
        public SimplexMode Mode;

        public SimplexStep Step;

        public int It = 0;
        public Fraction[,] Expressions;
        public string[] Vars;
        public (int pivotCol, int pivotRow, Fraction[] normalised, List<int> artificalIdx) Meta;

        public SimplexInterationRunner Start;

        public SimplexInterationRunner(
            SimplexStage stage,
            SimplexMode mode,
            Fraction[,] expressions,
            string[] vars,
            SimplexInterationRunner start = null,
            (int pivotCol, int pivotRow, Fraction[] normalised, List<int> artificalIdx)? meta = null,
            SimplexStep step = SimplexStep.PICK_PIVOT_COLUMN)
        {
            this.Stage = stage;
            this.Mode = mode;
            this.Step = step;
            this.Expressions = expressions.Clone() as Fraction[,];
            this.Vars = vars.Clone() as string[];
            this.It = (start is null) ? 0 : start.It;
            if (meta is null)
            {
                this.Meta = (-1, -1, null, new List<int>());
            } else
            {
                this.Meta = ((int, int, Fraction[], List<int>))meta;
            }
            if (start is null)
            {
                this.Start = this;
            }
            else
            {
                this.Start = start;
            }
        }

        public SimplexInterationRunner Clone()
        {
            return new SimplexInterationRunner
            (
                Stage,
                Mode,
                (Fraction[,])Expressions.Clone(),
                (string[])Vars.Clone(),
                Start,
                (Meta.pivotCol, Meta.pivotRow, Meta.normalised is null ? null : (Fraction[])Meta.normalised.Clone(), Meta.artificalIdx.ToList()),
                Step
            );
        }

        public SimplexRunnerOutput Next()
        {
            SimplexInterationRunner newRunner = Clone();
            switch (Step)
            {
                case SimplexStep.PICK_PIVOT_COLUMN:
                    {
                        int? result = GetColIdxByMode(Mode);
                        string pos = Mode == SimplexMode.MAX ? "negative" : "positive";
                        if (result is null)
                        {
                            if (IsCompleted())
                                return new SimplexRunnerOutput(SimplexState.ENDED, this, $"There are no {pos} (unoptimal) column in objective function, and therefore a solution is found");
                            else
                                return new SimplexRunnerOutput(SimplexState.FAILED, null, $"Contradictory result: No pivot column can be found but the solution isn't complete");
                        }
                        newRunner.Step = SimplexStep.PICK_PIVOT_ROW;
                        newRunner.Meta.pivotCol = (int)result;
                        return new SimplexRunnerOutput(SimplexState.NOT_ENDED, newRunner, $"Picked pivot column with index of: {result + Const.DisplayIndexOffset} as this is not optimal solution");
                    }
                case SimplexStep.PICK_PIVOT_ROW:
                    {
                        int? result = GetRowIdxByMode(Meta.pivotCol, Mode);
                        if (result is null)
                        {
                            return new SimplexRunnerOutput(SimplexState.FAILED, null, $"Cannot select a pivot row where RHS/col > 0 with pivot column with index: {Meta.pivotCol + Const.DisplayIndexOffset}");
                        }
                        newRunner.Step = SimplexStep.NORMALISE_ROW;
                        newRunner.Meta.pivotRow = (int)result;
                        return new SimplexRunnerOutput(SimplexState.NOT_ENDED, newRunner, $"Picked pivot row with index of: {result + Const.DisplayIndexOffset}");
                    }
                case SimplexStep.NORMALISE_ROW:
                    {
                        Fraction value = Expressions[Meta.pivotCol, Meta.pivotRow];
                        Fraction[] normalised = new Fraction[Expressions.GetLength(0)];
                        for (int x = 0; x < Expressions.GetLength(0); x++)
                        {
                            if (Const.SimplexNormaliseInpaceMod)
                                newRunner.Expressions[x, Meta.pivotRow] = Expressions[x, Meta.pivotRow] / value;
                            normalised[x] = Expressions[x, Meta.pivotRow] / value;
                        }
                        newRunner.Step = SimplexStep.APPLY_OTHER;
                        newRunner.Meta.normalised = normalised;
                        return new SimplexRunnerOutput(SimplexState.NOT_ENDED, newRunner, $"Normalise pivot row {Meta.pivotRow} by multiply the row with {1 / value}");
                    }
                case SimplexStep.APPLY_OTHER:
                    {
                        if (!Const.SimplexNormaliseInpaceMod)
                        {
#pragma warning disable CS0162
                            for (int x = 0; x < Expressions.GetLength(0); x++)
                            {
                                newRunner.Expressions[x, Meta.pivotRow] = Meta.normalised[x];
                            }
#pragma warning restore CS0162
                        }
                        for (int y = 0; y < Expressions.GetLength(1); y++)
                        {
                            if (y == Meta.pivotRow) continue;
                            Fraction value = -Expressions[Meta.pivotCol, y];
                            for (int x = 0; x < Expressions.GetLength(0); x++)
                            {
                                newRunner.Expressions[x, y] = Expressions[x, y] + value * Meta.normalised[x];
                            }
                        }
                        if (Stage == SimplexStage.TWO_STAGE_MIN || Stage == SimplexStage.TWO_STAGE_MAX)
                        {
                            newRunner.Step = SimplexStep.CHECK_ARTIFICAL;
                        }
                        else
                        {
                            newRunner.Step = SimplexStep.PICK_PIVOT_COLUMN;
                            newRunner.Meta = (-1, -1, null, new List<int>());
                            newRunner.Start = newRunner;
                            newRunner.It += 1;
                        }
                        return new SimplexRunnerOutput(SimplexState.NOT_ENDED, newRunner, "For each non-pivot row, put the new row as old_row + (-pivot_value)*pivot_row");
                    }
                case SimplexStep.CHECK_ARTIFICAL:
                    {
                        bool isCompleted = true;
                        List<int> artificalIdx = new List<int>();
                        for (int x = 0; x < Expressions.GetLength(0) - 1; x++)
                        {
                            string varName = Vars[x];
                            Fraction currValue = Expressions[x, 0];
                            if (varName == "A")
                            {
                                artificalIdx.Add(x);
                                if (currValue != 1)
                                {
                                    return new SimplexRunnerOutput(SimplexState.FAILED, null, "Var A on row 0 isn't 1");
                                }
                            }
                            else if (varName.StartsWith("a_") && varName.Length > 2 && int.TryParse(varName.Substring(2), out _))
                            {
                                artificalIdx.Add(x);
                                if (currValue != -1)
                                {
                                    isCompleted = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (currValue != 0)
                                {
                                    isCompleted = false;
                                    break;
                                }
                            }
                        }
                        string reason;
                        if (isCompleted)
                        {
                            newRunner.Step = SimplexStep.REMOVE_ARTIFICAL;
                            reason = "A = 0 => The tableau can transition to Stage 2 Simplex";
                        }
                        else
                        {
                            newRunner.Step = SimplexStep.PICK_PIVOT_COLUMN;
                            reason = "A != 0";
                            newRunner.Meta = (-1, -1, null, new List<int>());
                            newRunner.Start = newRunner;
                            newRunner.It += 1;
                        }
                        newRunner.Meta.artificalIdx = artificalIdx;
                        return new SimplexRunnerOutput(SimplexState.NOT_ENDED, newRunner, reason);
                    }
                case SimplexStep.REMOVE_ARTIFICAL:
                    {
                        var artificalIdx = newRunner.Meta.artificalIdx;
                        int[] notArtifical = Enumerable.Range(0, Expressions.GetLength(0)).Where(idx => !artificalIdx.Contains(idx)).ToArray();
                        Fraction[,] notArtificalExpr = new Fraction[notArtifical.Length, Expressions.GetLength(1) - 1];
                        for (int x = 0; x < notArtifical.Length; x++)
                        {
                            for (int y = 0; y < Expressions.GetLength(1) - 1; y++)
                            {
                                notArtificalExpr[x, y] = Expressions[notArtifical[x], y + 1];
                            }
                        }
                        var vars = this.Vars;
                        newRunner.Vars = notArtifical.Take(notArtifical.Length - 1).Select(idx => vars[idx]).ToArray(); // Exclude the last one which is RHS
                        newRunner.Stage = SimplexStage.ONE_STAGE;
                        newRunner.Expressions = notArtificalExpr;
                        newRunner.Mode = Stage == SimplexStage.TWO_STAGE_MIN ? SimplexMode.MIN : SimplexMode.MAX;
                        newRunner.Step = SimplexStep.PICK_PIVOT_COLUMN;
                        newRunner.Start = newRunner;
                            newRunner.It += 1;
                        newRunner.Meta = (-1, -1, null, new List<int>());
                        return new SimplexRunnerOutput(SimplexState.NOT_ENDED, newRunner, "Remove artifical variables");
                    }
                default:
                    {
                        return new SimplexRunnerOutput(SimplexState.FAILED, null, "Stuck at undefined state");
                    }
            }
        }

        private int? GetColIdxByMode(SimplexMode mode)
        {
            var expressions = this.Expressions;
            var selection = Enumerable.Range(1, expressions.GetLength(0) - 2)
                .Select(x => (x, expressions[x, 0]))
                .Select(((int i, Fraction frac) item) => mode == SimplexMode.MAX ? item : (item.i, -item.frac))
                .Where(item => item.Item2 < 0)
                .OrderBy(idxFrac => idxFrac.Item2)
                .ToArray();
            if (selection.Length == 0)
            {
                return null;
            }
            return selection[0].Item1;
        }

        private int? GetRowIdxByMode(int col, SimplexMode mode)
        {
            var expressions = this.Expressions;
            var selections = Enumerable.Range(1, expressions.GetLength(1) - 1)
                .Select(y => (y, expressions[col, y]))
                // .Select(item => mode == SimplexMode.MAX ? item : (item.y, -item.Item2))
                .Where(item => item.Item2 > 0 && expressions[expressions.GetLength(0) - 1, item.y] / item.Item2 > 0)
                .Select(item => (item.y, expressions[expressions.GetLength(0) - 1, item.y] / item.Item2))
                .OrderBy(idxFrac => idxFrac.Item2)
                .ToArray();
            if (selections.Length == 0)
                return null;
            return selections[0].y;
        }

        private bool IsCompleted()
        {
            var expressions = this.Expressions;
            var mode = this.Mode;
            return Vars[0] == "P" && Enumerable.Range(0, expressions.GetLength(0)).Select(x => expressions[x, 0]).All(value => mode == SimplexMode.MAX ? value >= 0 : value <= 0);
        }

        public Dictionary<string, Fraction> Resolve()
        {
            Dictionary<string, Fraction> resolved = new Dictionary<string, Fraction>();
            var expressions = this.Expressions;
            if (!IsCompleted())
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
                    resolved[Vars[x]] = expressions[expressions.GetLength(0) - 1, idx];
                }
            }
            return resolved;
        }

        public override string ToString()
        {
            var expressions = this.Expressions;
            string exprStr = Enumerable.Range(0, expressions.GetLength(1))
                .Select(y => string.Join(" & ", Enumerable.Range(0, expressions.GetLength(0)).Select(x => expressions[x, y].AsLatex())))
                .Select(row => $"{{ {row} }}")
                .Aggregate((a, b) => a + " \\\\\n" + b);
            return $"Stage: {Stage}\nMode: {Mode}\nStep: {Step}\nVars: [{string.Join(", ", Vars)}]\nExpressions:\n{exprStr}\nMeta: (pivotCol: {Meta.pivotCol}, pivotRow: {Meta.pivotRow}, normalised: [{(Meta.normalised is null ? "" : string.Join(", ", Meta.normalised))}], artificalIdx: [{string.Join(", ", Meta.artificalIdx)}])";
        }
    }
}
