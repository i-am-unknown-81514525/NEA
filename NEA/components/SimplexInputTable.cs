using ui.components.chainExt;
using ui.components;
using ui.utils;

namespace NEA.components
{
    public class SimplexInputTable : VirtualTable<FormattedTable>
    {
        public override (int x, int y) GetSize()
        {
            return (Inner.GetSize().x - 1, Inner.GetSize().y - 1);
        }

        protected override FormattedTable InnerConstructor()
        {
            var table = new FormattedTable((2, 2)).WithComponentConstructor(
                (FormattedTable _, (int x, int y) loc)
                    => loc.y == 0 ?
                        new SimplexTableauVariableField() :
                        (IComponent)new SimplexTableauValueField()
            );
            table[1, 0] = new TextLabel("RHS");
            return table;
        }

        public SimplexInputTable() : base() { }

        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            throw new System.InvalidOperationException();
        }

        public override void InsertColumn(int idx, SplitAmount amount = null)
        {
            throw new System.InvalidOperationException();
        }

        public override void AddColumn(SplitAmount amount = null)
        {
            int idx = Inner.GetSize().x - 1;
            Inner.InsertColumn(idx, amount);
            for (int y = 0; y < Inner.GetSize().y; y++)
            {
                if (y == 0)
                    Inner[idx, y] = new SimplexTableauVariableField();
                else
                    Inner[idx, y] = new SimplexTableauValueField();
            }
        }

        public override void AddRow(SplitAmount amount = null)
        {
            int idx = Inner.GetSize().y;
            Inner.AddRow(amount);
            for (int x = 0; x < Inner.GetSize().x; x++)
            {
                Inner[x, idx] = new SimplexTableauValueField();
            }
        }

        public override void RemoveColumn(int idx)
        {
            idx = Inner.GetSize().x - 2;
            Inner.RemoveColumn(idx);
        }

        public override void RemoveRow(int idx)
        {
            idx = Inner.GetSize().y - 1;
            Inner.RemoveRow(idx);
        }
    }
}