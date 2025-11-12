using NEA.math;
using ui.components;
using ui.components.chainExt;
// using ui.core;
using NEA.files;
using ui.utils;
using ui.fmt;
using System;

namespace NEA.components
{
    public class ImportPage : Container
    {
        public readonly Switcher outer_switcher;
        public readonly Switcher inner_switcher;

        public readonly SingleLineInputField filename_input = new SingleLineInputField();

        public readonly Container output_container = new Container() { new Padding() };

        public ImportPage(Switcher outer_switcher) : base()
        {
            this.outer_switcher = outer_switcher;
            Logger logger = new Logger()
                .WithHAlign<EmptyStore, Logger>(HorizontalAlignment.LEFT)
                .WithVAlign<EmptyStore, Logger>(VerticalAlignment.TOP)
                .WithForeground<EmptyStore, Logger>(ForegroundColorEnum.CYAN)
                .WithBackground<EmptyStore, Logger>(BackgroundColorEnum.BLACK);
            inner_switcher = new Switcher() {
                new VerticalGroupComponent() {
                    (filename_input, 1),
                    new Padding(),
                    (logger, 1),
                    (
                        new HorizontalGroupComponent() {
                            new PageSwitcher(outer_switcher, "Back", 0),
                            new Button("Import").WithHandler(
                                (_)=> {
                                    try
                                    {
                                        output_container.Set(
                                            new SimplexPagingOutputContainer(
                                                ToSimplexRunner.RunAll(
                                                    ImportHandler.ImportFromFile(filename_input.content)
                                                ),
                                                outer_switcher,
                                                inner_switcher
                                            )
                                        );
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Push(ex.Message);
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