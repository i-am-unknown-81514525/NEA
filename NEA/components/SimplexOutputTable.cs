using NEA.math;
using ui.components;
using ui.components.chainExt;
using ui.math;
using ui.utils;

namespace NEA.components
{
    public class VariableLabel : TextLabel
    {

        public VariableLabel(string text) : base(text)
        {
        }

        public override string AsLatex()
        {
            return $"${text}$";
        }
    }

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
        public SimplexInterationRunner Runner;

        protected override FormattedTable InnerConstructor()
        {
            return new FormattedTable((1, 1));
        }

        public SimplexOutputTable(SimplexInterationRunner runner)
        {
            Runner = runner;
            Inner.Resize((runner.Expressions.GetLength(0), runner.Expressions.GetLength(1) + 1));
            Inner.WithComponentConstructor(((int x, int y) loc) =>
            {
                if (loc.y == 0)
                {
                    if (loc.x < runner.Vars.Length)
                    {
                        return new VariableLabel(runner.Vars[loc.x]);
                    }

                    return new VariableLabel("RHS");
                }
                loc.y--;
                return new FractionTableCell(runner.Expressions[loc.x, loc.y]);
            });
        }

        public override void InsertColumn(int idx, SplitAmount amount = null)
        {
            Inner.InsertColumn(idx, amount);
        }

        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            Inner.InsertRow(idx, amount);
        }

        public override void RemoveColumn(int idx)
        {
            Inner.RemoveColumn(idx);
        }

        public override void RemoveRow(int idx)
        {
            Inner.RemoveRow(idx);
        }
    }
}
