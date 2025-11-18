using NEA.math;
using ui.components;
using ui.components.chainExt;
using ui.fmt;

namespace NEA.components
{
    public class StructuredPage : Container
    {
        public readonly Switcher OuterSwitcher;
        public readonly Switcher InnerSwitcher;

        public readonly SimplexStructuredInput StructuredInput = new SimplexStructuredInput();

        public readonly Container OutputContainer = new Container() { new Padding() };

        public readonly Logger Logger = new Logger().WithForeground<EmptyStore, Logger>(ForegroundColorEnum.RED);

        public StructuredPage(Switcher outerSwitcher) : base()
        {
            OuterSwitcher = outerSwitcher;
            InnerSwitcher = new Switcher() {
                new VerticalGroupComponent() {
                    StructuredInput,
                    (Logger, 1),
                    (
                        new HorizontalGroupComponent() {
                            new PageSwitcher(outerSwitcher, "Back", 0),
                            new Button("Start").WithHandler(
                                (_)=> {
                                    try {
                                        OutputContainer.Set(
                                            new SimplexPagingOutputContainer(
                                                ToSimplexRunner.RunAll(
                                                    ToSimplexRunner.Translate(
                                                        StructuredInput.Table.Extract()
                                                    )
                                                ),
                                                outerSwitcher,
                                                InnerSwitcher
                                            )
                                        );
                                    }  catch (System.Exception e){
                                        Logger.Push(e.Message);
                                        return;
                                    }
                                    InnerSwitcher.SwitchTo(1);
                                }
                            )
                        }
                    , 1)
                },
                OutputContainer
            };
            Add(InnerSwitcher);
        }
    }
}