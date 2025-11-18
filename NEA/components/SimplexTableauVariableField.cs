using NEA.store;
using ui.components;

namespace NEA.components
{
    public class SimplexTableauVariableField : SingleLineInputField<TableLocationStore>
    {
        public override TableLocationStore ComponentStoreConstructor()
        {
            return new TableLocationStore();
        }

        public SimplexTableauVariableField(string content = "") : base(content)
        {
            underline = false;
        }

        public override string AsLatex()
        {
            return $"${content}$";
        }
    }
}
