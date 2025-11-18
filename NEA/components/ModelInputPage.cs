using System;
using NEA.math;
using ui.components;
using ui.components.chainExt;
using ui.fmt;

namespace NEA.components
{
    public class ModelInputPage : Container
    {
        public readonly Switcher OuterSwitcher;
        public readonly Switcher InnerSwitcher;

        public readonly MultiLineInputField ModelInput = new MultiLineInputField("MAX ...\nST\n    ...\nEND");

        public readonly Container OutputContainer = new Container { new Padding() };

        public readonly Logger Logger = new Logger().WithForeground<EmptyStore, Logger>(ForegroundColorEnum.RED);

        public ModelInputPage(Switcher outerSwitcher)
        {
            OuterSwitcher = outerSwitcher;
            InnerSwitcher = new Switcher {
                new VerticalGroupComponent {
                    ModelInput,
                    (Logger, 1),
                    (
                        new HorizontalGroupComponent {
                            new PageSwitcher(outerSwitcher, "Back", 0),
                            new Button("Start").WithHandler(
                                _=> {
                                    try {
                                        OutputContainer.Set(
                                            new SimplexPagingOutputContainer(
                                                ToSimplexRunner.RunAll(
                                                    ToSimplexRunner.Translate(
                                                        ModelInput.content
                                                    )
                                                ),
                                                outerSwitcher,
                                                InnerSwitcher
                                            )
                                        );
                                    }  catch (Exception e){
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