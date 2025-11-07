using NEA.math;
using ui.components;
using ui.components.chainExt;

namespace NEA.components
{
    public class ModelInputPage : Container
    {
        public readonly Switcher outer_switcher;
        public readonly Switcher inner_switcher;

        public readonly MultiLineInputField model_input = new MultiLineInputField();

        public readonly Container output_container = new Container() { new Padding() };

        public ModelInputPage(Switcher outer_switcher) : base()
        {
            this.outer_switcher = outer_switcher;
            inner_switcher = new Switcher() {
                new VerticalGroupComponent() {
                    model_input,
                    (   
                        new HorizontalGroupComponent() {
                            new PageSwitcher(outer_switcher, "Back", 0),
                            new Button("Start").WithHandler(
                                (_)=> {
                                    output_container.Set(
                                        new SimplexPagingOutputContainer(
                                            ToSimplexRunner.RunAll(
                                                ToSimplexRunner.Translate(
                                                    model_input.content
                                                )
                                            ),
                                            outer_switcher,
                                            inner_switcher
                                        )
                                    );
                                    inner_switcher.SwitchTo(1);
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