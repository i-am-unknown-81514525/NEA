using NEA.store;
using ui.components;

namespace NEA.components
{
    public class SimplexTableauValueField : SingleLineInputField<TableLocationStore>
    {
        public override TableLocationStore ComponentStoreConstructor()
        {
            return new TableLocationStore();
        }

        public SimplexTableauValueField(string content = "") : base(content)
        {
            underline = false;
        }

        // protected override void OnExitHandler()
        // {
        //     if ((inputFieldHandler.GetContent() ?? "").Length > 0)
        //     {
        //         if (store.loc.y + 1 == store.parent.GetSize().y)
        //         {
        //             store.parent.InsertRow(store.loc.y + 1);
        //         }
        //         if (store.loc.x + 1 == store.parent.GetSize().x)
        //         {
        //             store.parent.InsertColumn(store.loc.x + 1);
        //         }
        //     }
        // }
    }
}
