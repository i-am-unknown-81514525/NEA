using NEA.math;
using ui.components;
using ui.components.chainExt;
using ui.fmt;

namespace NEA.components
{
    public class TableauPage : Container
    {
        public readonly Switcher OuterSwitcher;
        public readonly Switcher InnerSwitcher;

        public readonly SimplexTableauInput TableauInput = new SimplexTableauInput();

        public readonly Container OutputContainer = new Container() { new Padding() };

        public readonly Logger Logger = new Logger().WithForeground<EmptyStore, Logger>(ForegroundColorEnum.RED);

        public TableauPage(Switcher outerSwitcher) : base()
        {
            OuterSwitcher = outerSwitcher;
            InnerSwitcher = new Switcher() {
                new VerticalGroupComponent() {
                    TableauInput,
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
                                                        TableauInput.Table
                                                    )
                                                ),
                                                outerSwitcher,
                                                InnerSwitcher
                                            )
                                        );
                                    }
                                    catch(System.Exception e){
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