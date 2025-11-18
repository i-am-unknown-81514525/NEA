using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ui;
using ui.core;
using ui.test;
using ui.components;
using ui.components.chainExt;
using NEA.components;

namespace NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Run();
            // Prototype.Setup();
        }

        static void Run()
        {
            Switcher switcher = new Switcher();
            App app = new App(
                switcher
            );
            TableauPage tablePage = new TableauPage(switcher);
            ModelInputPage modelPage = new ModelInputPage(switcher);
            StructuredPage structuredPage = new StructuredPage(switcher);
            ImportPage importPage = new ImportPage(switcher);
            switcher.AddMulti(
                new IComponent[] {
                    new MainMenu(switcher),
                    modelPage,
                    tablePage,
                    structuredPage,
                    importPage
                }
            );
            app.WithExitHandler<EmptyStore, App>((appObj) =>
            {
                Console.WriteLine(appObj.Debug_WriteStructure());
                Console.WriteLine(ui.Debug.DebugStore.ToString());
                Console.WriteLine(tablePage.TableauInput.Table.AsLatex());
            }).Run();
        }
    }
}
