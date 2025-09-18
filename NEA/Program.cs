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
            TableauPage table_page = new TableauPage(switcher);
            switcher.AddMulti(
                new IComponent[] {
                    new MainMenu(switcher),
                    new VerticalGroupComponent() {
                        new MultiLineInputField(),
                        (
                            new HorizontalGroupComponent() {
                                new Button("Back")
                                    .WithHandler(
                                        (_)=>switcher.SwitchTo(0)
                                    ),
                                new Button("Start")
                            }
                        , 1)
                    },
                    table_page
                    
                }
            );
            app.WithExitHandler<EmptyStore, App>((appObj) =>
            {
                Console.WriteLine(appObj.Debug_WriteStructure());
                Console.WriteLine(ui.DEBUG.DebugStore.ToString());
                Console.WriteLine(table_page.tableau_input.table.AsLatex());
            }).Run();
        }
    }
}
