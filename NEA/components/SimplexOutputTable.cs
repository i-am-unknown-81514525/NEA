using ui.components;
using ui.components.chainExt;
using ui.utils;
using NEA.math;
using ui.math;

namespace NEA.components
{
    public class FractionTableCell : TextLabel
    {
        //Reactive of value with type Fraction, Trigger: SetHasUpdate();
        private Fraction _value;
        public Fraction value {get => _value; set {_value = value; text = value.ToString();} }

        public FractionTableCell(Fraction value) : base(value.ToString())
        {
            this.value = value;
        }

        public override string AsLatex()
        {
            return value.AsLatex();
        }
    }

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
                return new FractionTableCell(runner.expressions[loc.x, loc.y]);
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
