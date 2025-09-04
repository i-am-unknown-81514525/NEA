using ui.components;

namespace NEA.components
{
    public class SimplexTableauInput : Container
    {
        public readonly SimplexInputTable table = new SimplexInputTable();
        public readonly BoundedSpinner row = new BoundedSpinner("Row", 2, 2, 7);
        public readonly BoundedSpinner column = new BoundedSpinner("Column", 2, 2, 16);

        public SimplexTableauInput()
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