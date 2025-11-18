using System;
using System.IO;
using ui.components;
using ui.components.chainExt;

namespace NEA.components
{

    public class FileSaveBar : Container
    {
        public readonly TextLabel Label = new TextLabel("");
        public readonly TextLabel OverwriteMessage = new TextLabel("");

        public readonly SingleLineInputField FileNameInput = new SingleLineInputField();

        protected readonly Logger Logger;

        protected readonly Switcher Switcher = new Switcher();

        public readonly string SaveContent;

        protected void Save(bool force)
        {
            string filename = FileNameInput.content.Trim();
            string content = SaveContent ?? "";
            if (filename.Length == 0)
            {
                Logger?.Push("Filename cannot be empty.");
                Switcher.SwitchTo(0);
                return;
            }
            if (File.Exists(filename) && !force)
            {
                Switcher.SwitchTo(1);
                return;
            }
            try {
                File.WriteAllText(filename, content);
                Logger.Push($"File '{filename}' saved successfully.");
            } catch (Exception e){
                Logger.Push($"Error saving file '{filename}': {e.Message}");
            }
            Switcher.SwitchTo(0);
        }

        public FileSaveBar(string info, Logger logger = null, string saveContent = "")
        {
            Label.text = info;
            Logger = logger;
            SaveContent = saveContent;
            Switcher.Add(
                new HorizontalGroupComponent
                {
                    (Label, info.Length),
                    FileNameInput,
                    (new Button("Save").WithHandler(
                        _=> {
                            Save(false);
                        }
                    ), 6)
                }
            );
            Switcher.Add(
                new HorizontalGroupComponent
                {
                    OverwriteMessage,
                    (new Button("Back").WithHandler(
                        _=> {
                            Switcher.SwitchTo(0);
                        }
                    ), 6),
                    (new Button("Save Anyway").WithHandler(
                        _=> {
                            Save(true);
                        }
                    ), 13)
                }
            );
            Add(Switcher);
        }


    }

}