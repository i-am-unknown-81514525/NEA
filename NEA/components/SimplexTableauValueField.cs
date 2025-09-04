using System;
using NEA.store;
using ui;
using ui.components;
using ui.math;

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

        public override string AsLatex()
        {
            //bool ran = false;
            if (content == "")
            {
                content = "0";
                //ran = true;
            }
            //Console.WriteLine($"{content}, {content.Length}, {content is null}, {content == ""}, {ran}");
            if (Fraction.TryParse(content, out Fraction frac))
            {
                return frac.AsLatex();
            }
            //else
            //{
            //    Console.WriteLine($"Cannot parse '{content}' as a fraction");
            //}
            return base.AsLatex();
        }
    }
}
