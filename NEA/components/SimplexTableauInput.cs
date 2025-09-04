using ui.components;

namespace NEA.components
{
    public class SimplexTableauInput : Container
    {
        public readonly SimplexInputTable table = new SimplexInputTable();
        public readonly BoundedSpinner row = new BoundedSpinner("Row", 1, 1, 6);
        public readonly BoundedSpinner column = new BoundedSpinner("Column", 1, 1, 15);

        public SimplexTableauInput()
        {
            row.onChange = (value) => table.ForceResize((table.GetSize().x, value));
            column.onChange = (value) => table.ForceResize((value, table.GetSize().y));
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