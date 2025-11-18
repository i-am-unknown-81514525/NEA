using ui.components;

namespace NEA.components
{
    public class SimplexStructuredInput : Container
    {
        public readonly StructuredInputTable Table = new StructuredInputTable();
        public readonly BoundedSpinner Row = new BoundedSpinner("Constraints", 1, 1, 6);
        public readonly BoundedSpinner Column = new BoundedSpinner("Variables", 1, 1, 8);

        public SimplexStructuredInput()
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