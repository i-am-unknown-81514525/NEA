using NEA.math;
using ui.components;
using ui.components.chainExt;

namespace NEA.components
{
    public class TableauPage : Container
    {
        Switcher outer_switcher;
        Switcher inner_switcher;

        public readonly SimplexTableauInput tableau_input = new SimplexTableauInput();

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
                                        ToSimplexRunner.RunAll(
                                            ToSimplexRunner.Translate(
                                                tableau_input.table
                                            )
                                        );
                                    }
                                )
                            }
                        , 1)
                    }
            };
            Add(inner_switcher);
        }
    }
}