using ui.components;
using ui.components.chainExt;
using ui.math;

namespace NEA.components
{
    public class InnerMenu : Container
    {
        public InnerMenu(Switcher switcher) : base()
        {
            Add(
                new VerticalGroupComponent() {
                    (new Frame().WithInner(new Button("Structured input")), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new Button("Tableau input").WithHandler((_)=>switcher.SwitchTo(2))), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new Button("LP model input").WithHandler((_)=>switcher.SwitchTo(1))), new Fraction(1, 4)),
                    // (new Frame().WithInner(new Button("Button 3")), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new ExitButton("Exit")), new Fraction(1, 4)),
                }
            );
        }
    }


    public class MainMenu : Container
    {
        public MainMenu(Switcher switcher) : base()
        {
            Add(
                new Frame(
                    (new TextLabel("Simplex Solver Menu"), 19)
                ).WithInner(
                    new VerticalGroupComponent() {
                        (new Padding(), new Fraction(3, 8)),
                        (
                            new HorizontalGroupComponent() {
                                (new Padding(), new Fraction(1, 3)),
                                (new InnerMenu(switcher), new Fraction(1, 3)),
                                (new Padding(), new Fraction(1, 3))
                            },
                            19 // new Fraction(3, 8)
                        ),
                        (new Padding(), new Fraction(2, 8))
                    }
                )
            );
        }
    }
}
