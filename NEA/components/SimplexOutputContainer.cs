using System.Linq;
using NEA.files;
using NEA.math;
using ui.components;
using ui.components.chainExt;
using ui.fmt;
using ui.utils;

namespace NEA.components
{
    public struct SimplexOutputMeta
    {
        public int idx { get; set; }
        public int stage { get; set; }
        public int it { get; set; }
        public int itIdx { get; set; }
    }

    public class SimplexOutputContainer : Container
    {
        public SimplexState State;
        public SimplexInterationRunner Runner;
        public SimplexOutputTable Table;
        public string reason
        {
            get => ReasonLabel.text;
            set
            {
                ReasonLabel.text = value;
                SetHasUpdate();
            }
        }
        public TextLabel ReasonLabel = new TextLabel();

        public readonly Logger Logger = new Logger()
                .WithHAlign<EmptyStore, Logger>(HorizontalAlignment.LEFT)
                .WithVAlign<EmptyStore, Logger>(VerticalAlignment.TOP)
                .WithForeground<EmptyStore, Logger>(ForegroundColorEnum.YELLOW)
                .WithBackground<EmptyStore, Logger>(BackgroundColorEnum.BLACK);
        public SimplexOutputContainer(SimplexState state, SimplexInterationRunner runner, string reason)
        {
            State = state;
            Runner = runner;
            string stageName = "ONE";
            if (new[] { SimplexStage.TWO_STAGE_MAX, SimplexStage.TWO_STAGE_MIN }.Contains(runner.Stage))
            {
                stageName = "TWO";
            }
            this.reason = $"it:{runner.It+1}, Step:{(int)runner.Step}, Stage:{stageName}: {reason}";
            // SingleLineInputField filename_input = new SingleLineInputField();
            Logger.Push(reason);
            Add(
                new VerticalGroupComponent
                {
                    (Table = new SimplexOutputTable(runner)),
                    (Logger, 1),
                    (
                        (new FileSaveBar("Export filename: ", Logger, ExportHandler.ExportToContent(runner)), 1)
                    )
                }
            );
        }

        protected override void OnVisibleInternal()
        {
            base.OnMount();
            Logger.WithHAlign<EmptyStore, Logger>(HorizontalAlignment.LEFT)
                .WithVAlign<EmptyStore, Logger>(VerticalAlignment.TOP)
                .WithForeground<EmptyStore, Logger>(ForegroundColorEnum.YELLOW)
                .WithBackground<EmptyStore, Logger>(BackgroundColorEnum.BLACK);
            Logger.Push(reason);
        }

        public override string AsLatex()
        {
            return Table.AsLatex();
        }
    }
}
