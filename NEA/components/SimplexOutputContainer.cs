using ui.components;
using NEA.math;
using ui.math;
using ui.components.chainExt;
using ui.utils;
using ui.fmt;
using System;
using NEA.files;

namespace NEA.components
{
    public struct SimplexOutputMeta
    {
        public int idx { get; set; }
        public int stage { get; set; }
        public int it { get; set; }
        public int it_idx { get; set; }
    }

    public class SimplexOutputContainer : Container
    {
        public SimplexState state;
        public SimplexInterationRunner runner;
        public SimplexOutputTable table;
        public string reason
        {
            get => reasonLabel.text;
            set
            {
                reasonLabel.text = value;
                SetHasUpdate();
            }
        }
        public TextLabel reasonLabel = new TextLabel();

        public readonly Logger logger = new Logger()
                .WithHAlign<EmptyStore, Logger>(HorizontalAlignment.LEFT)
                .WithVAlign<EmptyStore, Logger>(VerticalAlignment.TOP)
                .WithForeground<EmptyStore, Logger>(ForegroundColorEnum.YELLOW)
                .WithBackground<EmptyStore, Logger>(BackgroundColorEnum.BLACK);
        public SimplexOutputContainer(SimplexState state, SimplexInterationRunner runner, string reason)
        {
            this.state = state;
            this.runner = runner;
            this.reason = reason;
            // SingleLineInputField filename_input = new SingleLineInputField();
            logger.Push(reason);
            Add(
                new VerticalGroupComponent()
                {
                    (table = new SimplexOutputTable(runner)),
                    (logger, 1),
                    (
                        (new FileSaveBar("Export filename: ", logger, ExportHandler.ExportToContent(runner)), 1)
                    )
                }
            );
        }

        protected override void OnVisibleInternal()
        {
            base.OnMount();
            logger.WithHAlign<EmptyStore, Logger>(HorizontalAlignment.LEFT)
                .WithVAlign<EmptyStore, Logger>(VerticalAlignment.TOP)
                .WithForeground<EmptyStore, Logger>(ForegroundColorEnum.YELLOW)
                .WithBackground<EmptyStore, Logger>(BackgroundColorEnum.BLACK);
            logger.Push(reason);
        }

        public override string AsLatex()
        {
            return table.AsLatex();
        }
    }
}
