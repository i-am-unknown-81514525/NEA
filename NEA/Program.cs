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
            switcher.AddMulti(
                new IComponent[] {
                    new NEA.components.MainMenu(switcher),
                    new VerticalGroupComponent() {
                        new MultiLineInputField(),
                        (new Button("Back")
                            .WithHandler(
                                (_)=>switcher.SwitchTo(0)
                            ), 1)
                    },
                    new VerticalGroupComponent() {
                        (
                            new FormattedTable(
                                (5, 5)
                            ) {}
                        ).WithComponentConstructor(
                            () => new SingleLineInputField().WithChange((c) => c.underline = false)
                        ),
                        (new Button("Back")
                            .WithHandler(
                                (_)=>switcher.SwitchTo(0)
                            ), 1)
                    }
                }
            );
            app.WithExitHandler<EmptyStore, App>((appObj) =>
            {
                Console.WriteLine(appObj.Debug_WriteStructure());
                Console.WriteLine(ui.DEBUG.DebugStore.ToString());
            }).Run();
        }
    }
}
