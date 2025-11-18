using System;
using System.Linq;
using System.Text;
using NEA.math;
using ui.components;
using ui.components.chainExt;
using ui.core;
using ui.fmt;
using ui.math;
using ui.utils;

namespace NEA.components
{
    public class SimplexPagingOutputContainer : Container
    {
        public readonly Switcher OuterSwitcher;
        public readonly Switcher TableauSwitcher;
        public readonly Switcher Switcher = new Switcher();

        public SimplexPagingOutputContainer(SimplexRunnerOutput[] outputs, Switcher outerSwitcher, Switcher tableauSwitcher)
        {
            OuterSwitcher = outerSwitcher;
            TableauSwitcher = tableauSwitcher;

            if (outputs.Length == 0 || outputs.Length == 1)
            {
                IComponent item = new TextLabel("No outputs generated. (Unexpected)");
                if (outputs.Length == 1)
                {
                    item = outputs[0].ToOutputContainer();
                }
                Add(new VerticalGroupComponent
                {
                    (item, new Fraction(1, 1)),
                    (
                        new SimplexOutputExitContainer(
                            (outerSwitcher, 0, "Back to Menu"),
                            (tableauSwitcher, 0, "Back to Tableau")
                        ),
                        1
                    )
                });
                return;
            }
            Switcher.Add(
                new VerticalGroupComponent
                {
                    (outputs[0].ToOutputContainer(), new Fraction(1, 1)),
                    (
                        new HorizontalGroupComponent {
                            (new SimplexOutputExitContainer(
                                (outerSwitcher, 0, "Back to Menu"),
                                (tableauSwitcher, 0, "Back to Tableau")
                            ), new Fraction(2, 3)),
                            (new PageSwitcher(Switcher, "Next", 1), new Fraction(1, 3))
                        },1
                    )
                }
            );
            for (int i = 1; i < outputs.Length - 1; i++)
            {
                int idx = i;
                Switcher.Add(
                    new VerticalGroupComponent
                    {
                        (outputs[idx].ToOutputContainer(), new Fraction(1, 1)),
                        (
                            new HorizontalGroupComponent {
                                (new PageSwitcher(Switcher, "Last", idx - 1), new Fraction(1, 4)),
                                (new SimplexOutputExitContainer(
                                    (outerSwitcher, 0, "Back to Menu"),
                                    (tableauSwitcher, 0, "Back to Tableau")
                                ), new Fraction(2, 4)),
                                (new PageSwitcher(Switcher, "Next", idx + 1), new Fraction(1, 4))
                            },1
                        )
                    }
                );
            }
            int lastIdx = outputs.Length - 1;
            Switcher.Add(
                new VerticalGroupComponent
                {
                    (outputs[lastIdx].ToOutputContainer(), new Fraction(1, 1)),
                    (
                        new HorizontalGroupComponent {
                            (new PageSwitcher(Switcher, "Last", lastIdx - 1), new Fraction(1, 3)),
                            (new SimplexOutputExitContainer(
                                (outerSwitcher, 0, "Back to Menu"),
                                (tableauSwitcher, 0, "Back to Tableau")
                            ), new Fraction(2, 3)),
                            (new PageSwitcher(Switcher, "Result", lastIdx + 1), new Fraction(1, 4))
                        }, 1
                    )
                }
            );
            Logger outputLogger = new Logger()
                .WithHAlign<EmptyStore, Logger>(HorizontalAlignment.LEFT)
                .WithVAlign<EmptyStore, Logger>(VerticalAlignment.TOP)
                .WithForeground<EmptyStore, Logger>(ForegroundColorEnum.CYAN)
                .WithBackground<EmptyStore, Logger>(BackgroundColorEnum.BLACK);
            Switcher.Add(
                new VerticalGroupComponent
                {
                    (
                        new TextLabel(
                            string.Join("\n", (outputs[lastIdx].Next).Resolve().Select(p=>$"{p.Key}: {p.Value}"))
                        ).WithVAlign<EmptyStore, TextLabel>(VerticalAlignment.TOP)
                        .WithHAlign<EmptyStore, TextLabel>(HorizontalAlignment.LEFT),
                        new Fraction(1, 1)
                    ),
                    (outputLogger, 1),
                    (new FileSaveBar("Export latex file:", outputLogger, ToLatex(outputs)), 1),
                    (
                        new HorizontalGroupComponent {
                            (new PageSwitcher(Switcher, "Last", lastIdx), new Fraction(1, 4)),
                            (new SimplexOutputExitContainer(
                                (outerSwitcher, 0, "Back to Menu"),
                                (tableauSwitcher, 0, "Back to Tableau")
                            ), new Fraction(2, 4)),
                            (new Button("Open As Latex").WithHandler((button, loc) => { DisplayLatex(ToLatex(outputs)); }), new Fraction(1, 4))
                        },1
                    )
                }
            );

            Add(Switcher);
        }

        public static string ToLatex(SimplexRunnerOutput[] outputs)
        {
            string baseString = @"\documentclass{{article}}
\usepackage{{amsmath}}
\usepackage{{mathtools}}
\begin{{document}}
\noindent
{0}
\end{{document}}";
            string content = string.Join("\\\\\\\\\n", outputs.Select(output => output.AsLatex()));
            return string.Format(baseString, content);
        }

        public static void DisplayLatex(string latex)
        {
            string urlBase = "https://www.overleaf.com/docs?snip_uri[]=data:application/x-tex;base64,{0}&snip_name[]=main.tex";
            string encoded = Base64Encode(latex).Replace("+", "-").Replace("/", "_").Replace("=", "");
            string fullUrl = string.Format(urlBase, encoded);
            ConsoleHandler.ConsoleIntermediateHandler.OpenWebsite(fullUrl);
        }


        // https://stackoverflow.com/a/11743162 CC-BY-SA 4.0 10/11/2025
        public static string Base64Encode(string plainText) 
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        
    }
}