using ui.components;

namespace NEA.components
{
    public class SimplexTableauInput : Container
    {
        public readonly SimplexInputTable Table = new SimplexInputTable();
        public readonly BoundedSpinner Row = new BoundedSpinner("Row", 2, 2, 7);
        public readonly BoundedSpinner Column = new BoundedSpinner("Column", 2, 2, 16);

        public SimplexTableauInput()
        {
            Row.OnChange = (value) => Table.ForceResize((Table.GetSize().x, value));
            Column.OnChange = (value) => Table.ForceResize((value, Table.GetSize().y));
            Table.Resize((Column.amount, Row.amount));
            Add(
                new VerticalGroupComponent()
                {
                    Table,
                    (new HorizontalGroupComponent() {
                        Row,
                        Column
                    }, 1)
                }
            );
        }
    }
}