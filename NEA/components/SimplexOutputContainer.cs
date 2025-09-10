using ui.components;
using NEA.math;
using ui.math;

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
        public SimplexOutputContainer(SimplexState state, SimplexInterationRunner runner, string reason)
        {
            this.state = state;
            this.runner = runner;
            this.reason = reason;
            Add(
                new VerticalGroupComponent()
                {
                    new SimplexOutputTable(runner)
                }
            );
        }
    }
}
