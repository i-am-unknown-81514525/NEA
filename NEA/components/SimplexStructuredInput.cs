using ui.components;

namespace NEA.components
{
    public class SimplexStructuredInput : Container
    {
        public readonly StructuredInputTable table = new StructuredInputTable();
        public readonly BoundedSpinner row = new BoundedSpinner("Constraints", 1, 1, 6);
        public readonly BoundedSpinner column = new BoundedSpinner("Variables", 1, 1, 8);

        public SimplexStructuredInput()
        {
            row.onChange = (value) => table.ForceResize((table.GetSize().x, value));
            column.onChange = (value) => table.ForceResize((value, table.GetSize().y));
            table.Resize((column.amount, row.amount));
            Add(
                new VerticalGroupComponent()
                {
                    table,
                    (new HorizontalGroupComponent() {
                        row,
                        column
                    }, 1)
                }
            );
        }
    }
}