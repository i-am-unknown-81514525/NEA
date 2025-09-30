using NEA.math;
using ui.components;
using ui.components.chainExt;

namespace NEA.components
{
    public class TableauPage : Container
    {
        public readonly Switcher outer_switcher;
        public readonly Switcher inner_switcher;

        public readonly SimplexTableauInput tableau_input = new SimplexTableauInput();

        public readonly Container output_container = new Container() { new Padding() };

        public TableauPage(Switcher outer_switcher) : base()
        {
            this.outer_switcher = outer_switcher;
            inner_switcher = new Switcher() {
                new VerticalGroupComponent() {
                    tableau_input,
                    (   
                        new HorizontalGroupComponent() {
                            new PageSwitcher(outer_switcher, "Back", 0),
                            new Button("Start").WithHandler(
                                (_)=> {
                                    output_container.Set(
                                        new SimplexPagingOutputContainer(
                                            ToSimplexRunner.RunAll(
                                                ToSimplexRunner.Translate(
                                                    tableau_input.table
                                                )
                                            )
                                        )
                                    );
                                }
                            )
                        }
                    , 1)
                },
                output_container
            };
            Add(inner_switcher);
        }
    }
}