using NEA.math;
using ui.components;
using ui.components.chainExt;
using ui.fmt;

namespace NEA.components
{
    public class StructuredPage : Container
    {
        public readonly Switcher outer_switcher;
        public readonly Switcher inner_switcher;

        public readonly SimplexStructuredInput structured_input = new SimplexStructuredInput();

        public readonly Container output_container = new Container() { new Padding() };

        public readonly Logger logger = new Logger().WithForeground<EmptyStore, Logger>(ForegroundColorEnum.RED);

        public StructuredPage(Switcher outer_switcher) : base()
        {
            this.outer_switcher = outer_switcher;
            inner_switcher = new Switcher() {
                new VerticalGroupComponent() {
                    structured_input,
                    (logger, 1),
                    (
                        new HorizontalGroupComponent() {
                            new PageSwitcher(outer_switcher, "Back", 0),
                            new Button("Start").WithHandler(
                                (_)=> {
                                    try {
                                        output_container.Set(
                                            new SimplexPagingOutputContainer(
                                                ToSimplexRunner.RunAll(
                                                    ToSimplexRunner.Translate(
                                                        structured_input.table.Extract()
                                                    )
                                                ),
                                                outer_switcher,
                                                inner_switcher
                                            )
                                        );
                                    }  catch (System.Exception e){
                                        logger.Push(e.Message);
                                        return;
                                    }
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