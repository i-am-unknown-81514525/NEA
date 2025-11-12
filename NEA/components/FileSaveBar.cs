using ui.components;
using ui.components.chainExt;

namespace NEA.components
{

    public class FileSaveBar : Container
    {
        public readonly TextLabel label = new TextLabel("");
        public readonly TextLabel overwrite_message = new TextLabel("");

        public readonly SingleLineInputField file_name_input = new SingleLineInputField("");

        protected readonly Logger logger = null;

        protected readonly Switcher switcher = new Switcher();

        public string save_content = "";

        protected void Save(bool force)
        {
            string filename = file_name_input.content.Trim();
            string content = save_content ?? "";
            if (filename.Length == 0)
            {
                logger?.Push("Filename cannot be empty.");
                switcher.SwitchTo(0);
                return;
            }
            if (System.IO.File.Exists(filename) && !force)
            {
                switcher.SwitchTo(1);
                return;
            }
            try {
                System.IO.File.WriteAllText(filename, content);
                logger.Push($"File '{filename}' saved successfully.");
            } catch (System.Exception e){
                logger.Push($"Error saving file '{filename}': {e.Message}");
            }
            switcher.SwitchTo(0);
        }

        public FileSaveBar(string info, Logger logger = null, string save_content = "") : base()
        {
            label.text = info;
            this.logger = logger;
            this.save_content = save_content;
            switcher.Add(
                new HorizontalGroupComponent()
                {
                    (label, info.Length),
                    file_name_input,
                    new Button("Save").WithHandler(
                        (_)=> {
                            Save(false);
                        }
                    )
                }
            );
            switcher.Add(
                new HorizontalGroupComponent()
                {
                    overwrite_message,
                    (new Button("Back").WithHandler(
                        (_)=> {
                            switcher.SwitchTo(0);
                        }
                    ), 6),
                    (new Button("Save Anyway").WithHandler(
                        (_)=> {
                            Save(true);
                        }
                    ), 13)
                }
            );
            Add(switcher);
        }


    }

}