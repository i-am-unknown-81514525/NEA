using ui.components;
using ui.components.chainExt;
using ui.utils;
using NEA.math;

namespace NEA.components
{
    public class SimplexOutputTable : VirtualTable<FormattedTable>
    {
        public SimplexInterationRunner runner;

        protected override FormattedTable InnerConstructor()
        {
            return new FormattedTable((1, 1));
        }

        public SimplexOutputTable(SimplexInterationRunner runner) : base()
        {
            this.runner = runner;
            inner.Resize((runner.expressions.GetLength(0), runner.expressions.GetLength(1) + 1));
            inner.WithComponentConstructor(((int x, int y) loc) =>
            {
                if (loc.y == 0)
                {
                    if (loc.x < runner.vars.Length)
                    {
                        return new TextLabel(runner.vars[loc.x]);
                    }
                    else
                    {
                        return new TextLabel("RHS");
                    }
                }
                loc.y--;
                return new TextLabel(runner.expressions[loc.x, loc.y].AsLatex());
            });
        }

        public override void InsertColumn(int idx, SplitAmount amount = null)
        {
            inner.InsertColumn(idx, amount);
        }

        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            inner.InsertRow(idx, amount);
        }

        public override void RemoveColumn(int idx)
        {
            inner.RemoveColumn(idx);
        }

        public override void RemoveRow(int idx)
        {
            inner.RemoveRow(idx);
        }
    }
}
