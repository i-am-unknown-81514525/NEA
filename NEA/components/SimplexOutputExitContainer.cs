using ui.components;
using ui.components.chainExt;

namespace NEA.components
{
    public class SimplexOutputExitContainer : Container
    {
        public SimplexOutputExitContainer((Switcher switcher, int page, string name) menuSwitcher, (Switcher switcher, int page, string name) optionsSwitcher)
        {
            Add(new HorizontalGroupComponent
            {
                new Button(menuSwitcher.name).WithHandler(
                    _=>{
                        menuSwitcher.switcher.SwitchTo(menuSwitcher.page);
                        optionsSwitcher.switcher.SwitchTo(optionsSwitcher.page);
                    }
                ),
                new Button(optionsSwitcher.name).WithHandler(
                    _=>optionsSwitcher.switcher.SwitchTo(optionsSwitcher.page)
                )
            });

        }
    }
}