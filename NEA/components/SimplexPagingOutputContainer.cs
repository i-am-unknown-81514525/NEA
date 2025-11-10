using ui.components;
using NEA.math;
using ui.components.chainExt;
using ui.math;
using System.Linq;

namespace NEA.components
{
    public class SimplexPagingOutputContainer : Container
    {
        public readonly Switcher outer_switcher;
        public readonly Switcher tableau_switcher;
        public readonly Switcher switcher = new Switcher();

        public SimplexPagingOutputContainer(SimplexRunnerOutput[] outputs, Switcher outer_switcher, Switcher tableau_switcher)
        {
            this.outer_switcher = outer_switcher;
            this.tableau_switcher = tableau_switcher;

            if (outputs.Length == 0 || outputs.Length == 1)
            {
                IComponent item = new TextLabel("No outputs generated. (Unexpected)");
                if (outputs.Length == 1)
                {
                    item = outputs[0].ToOutputContainer();
                }
                Add(new VerticalGroupComponent()
                {
                    (item, new Fraction(1, 1)),
                    (
                        new SimplexOutputExitContainer(
                            (outer_switcher, 0, "Back to Menu"),
                            (tableau_switcher, 0, "Back to Tableau")
                        ),
                        1
                    )
                });
                return;
            }
            switcher.Add(
                new VerticalGroupComponent()
                {
                    (outputs[0].ToOutputContainer(), new Fraction(1, 1)),
                    (
                        new HorizontalGroupComponent() {
                            (new SimplexOutputExitContainer(
                                (outer_switcher, 0, "Back to Menu"),
                                (tableau_switcher, 0, "Back to Tableau")
                            ), new Fraction(2, 3)),
                            (new PageSwitcher(switcher, "Next", 1), new Fraction(1, 3))
                        },1
                    )
                }
            );
            for (int i = 1; i < outputs.Length - 1; i++)
            {
                int idx = i;
                switcher.Add(
                    new VerticalGroupComponent()
                    {
                        (outputs[idx].ToOutputContainer(), new Fraction(1, 1)),
                        (
                            new HorizontalGroupComponent() {
                                (new PageSwitcher(switcher, "Last", idx - 1), new Fraction(1, 4)),
                                (new SimplexOutputExitContainer(
                                    (outer_switcher, 0, "Back to Menu"),
                                    (tableau_switcher, 0, "Back to Tableau")
                                ), new Fraction(2, 4)),
                                (new PageSwitcher(switcher, "Next", idx + 1), new Fraction(1, 4))
                            },1
                        )
                    }
                );
            }
            int last_idx = outputs.Length - 1;
            switcher.Add(
                new VerticalGroupComponent()
                {
                    (outputs[last_idx].ToOutputContainer(), new Fraction(1, 1)),
                    (
                        new HorizontalGroupComponent() {
                            (new PageSwitcher(switcher, "Last", last_idx - 1), new Fraction(1, 3)),
                            (new SimplexOutputExitContainer(
                                (outer_switcher, 0, "Back to Menu"),
                                (tableau_switcher, 0, "Back to Tableau")
                            ), new Fraction(2, 3)),
                            (new PageSwitcher(switcher, "Result", last_idx + 1), new Fraction(1, 4))
                        }, 1
                    )
                }
            );
            switcher.Add(
                new VerticalGroupComponent()
                {
                    (
                        new TextLabel(
                            string.Join("\n", ((SimplexInterationRunner)outputs[last_idx].next).Resolve().Select(p=>$"{p.Key}: {p.Value}"))
                        ).WithVAlign<EmptyStore, TextLabel>(ui.utils.VerticalAlignment.TOP)
                        .WithHAlign<EmptyStore, TextLabel>(ui.utils.HorizontalAlignment.LEFT),
                        new Fraction(1, 1)
                    ),
                    (
                        new HorizontalGroupComponent() {
                            (new PageSwitcher(switcher, "Last", last_idx), new Fraction(1, 4)),
                            (new SimplexOutputExitContainer(
                                (outer_switcher, 0, "Back to Menu"),
                                (tableau_switcher, 0, "Back to Tableau")
                            ), new Fraction(2, 4)),
                            (new Button("Open As Latex").WithHandler((button, loc) => { DisplayLatex(ToLatex(outputs)); }), new Fraction(1, 4))
                        },1
                    )
                }
            );

            Add(switcher);
        }

        public static string ToLatex(SimplexRunnerOutput[] outputs)
        {
            string base_string = @"\documentclass{{article}}
\usepackage{{amsmath}}
\usepackage{{mathtools}}
\begin{{document}}
\text{{Begin}}
{0}
\end{{document}}";
            string content = string.Join("\\\n", outputs.Select(output => output.AsLatex()));
            return string.Format(base_string, content);
        }

        public static void DisplayLatex(string latex)
        {
            string urlBase = "https://www.overleaf.com/docs?snip_uri[]=data:application/x-tex;base64,{0}&snip_name[]=main.tex";
            string encoded = Base64Encode(latex).Replace("+", "-").Replace("/", "_").Replace("=", "");
            string fullURL = string.Format(urlBase, encoded);
            ui.core.ConsoleHandler.ConsoleIntermediateHandler.OpenWebsite(fullURL);
        }


        // https://stackoverflow.com/a/11743162 CC-BY-SA 4.0 10/11/2025
        public static string Base64Encode(string plainText) 
        {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        
    }
}