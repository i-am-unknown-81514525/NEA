using ui.components;
using ui.components.chainExt;

namespace NEA.components
{
    public class SimplexOutputExitContainer : Container
    {
        public SimplexOutputExitContainer((Switcher switcher, int page, string name) menu_switcher, (Switcher switcher, int page, string name) options_switcher)
        {
            Add(new HorizontalGroupComponent()
            {
                new Button(menu_switcher.name).WithHandler(
                    (_)=>{
                        menu_switcher.switcher.SwitchTo(menu_switcher.page);
                        options_switcher.switcher.SwitchTo(options_switcher.page);
                    }
                ),
                new Button(options_switcher.name).WithHandler(
                    (_)=>options_switcher.switcher.SwitchTo(options_switcher.page)
                )
            });

        }
    }
}