using System;
using NEA.files;
using NEA.math;
using ui.components;
using ui.components.chainExt;
using ui.fmt;
using ui.utils;
// using ui.core;

namespace NEA.components
{
    public class ImportPage : Container
    {
        public readonly Switcher OuterSwitcher;
        public readonly Switcher InnerSwitcher;

        public readonly SingleLineInputField FilenameInput = new SingleLineInputField();

        public readonly Container OutputContainer = new Container { new Padding() };

        public ImportPage(Switcher outerSwitcher)
        {
            OuterSwitcher = outerSwitcher;
            Logger logger = new Logger()
                .WithHAlign<EmptyStore, Logger>(HorizontalAlignment.LEFT)
                .WithVAlign<EmptyStore, Logger>(VerticalAlignment.TOP)
                .WithForeground<EmptyStore, Logger>(ForegroundColorEnum.CYAN)
                .WithBackground<EmptyStore, Logger>(BackgroundColorEnum.BLACK);
            InnerSwitcher = new Switcher {
                new VerticalGroupComponent {
                    new Padding(),
                    (new HorizontalGroupComponent
                    {
                        (new TextLabel("Filename: "), 10),
                        FilenameInput
                    }, 1),
                    (logger, 1),
                    (
                        new HorizontalGroupComponent {
                            new PageSwitcher(outerSwitcher, "Back", 0),
                            new Button("Import").WithHandler(
                                _=> {
                                    try
                                    {
                                        OutputContainer.Set(
                                            new SimplexPagingOutputContainer(
                                                ToSimplexRunner.RunAll(
                                                    ImportHandler.ImportFromFile(FilenameInput.content)
                                                ),
                                                outerSwitcher,
                                                InnerSwitcher
                                            )
                                        );
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Push(ex.Message);
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