using ui.components.chainExt;
using ui.components;
using ui.utils;
using math_parser.atom;
using System.Collections.Generic;
using math_parser.tokenizer;
using math_parser.math;

namespace NEA.components
{
    public class ComparsionStateStore : ComponentStore
    {
        public math_parser.atom.ComparsionSymbolAtom atom;

        public ComparsionStateStore(math_parser.atom.ComparsionSymbolAtom atom) : base()
        {
            this.atom = atom;
        }
    }

    public class ComparisionStateButton : Button<ComparsionStateStore>
    {
        public override ComparsionStateStore ComponentStoreConstructor()
        {
            return new ComparsionStateStore(null);
        }

        public ComparisionStateButton(ComparsionStateStore stateStore = null) : base()
        {
            store.atom = stateStore.atom ?? math_parser.atom.ComparsionSymbolAtom.Le;
            onClickHandler = (btn, loc) =>
            {
                btn.store.atom = (
                    new Dictionary<math_parser.atom.ComparsionSymbolAtom, math_parser.atom.ComparsionSymbolAtom>()
                    {
                        { math_parser.atom.ComparsionSymbolAtom.Le, math_parser.atom.ComparsionSymbolAtom.Ge },
                        { math_parser.atom.ComparsionSymbolAtom.Ge, math_parser.atom.ComparsionSymbolAtom.Eq },
                        { math_parser.atom.ComparsionSymbolAtom.Eq, math_parser.atom.ComparsionSymbolAtom.Le },
                    }
                )[btn.store.atom];

            };
        }
    }

    public class StructuredInputTable : VirtualTable<Table>
    {
        public override (int x, int y) GetSize()
        {
            return ((inner.GetSize().x - 5) / 3 + 1, inner.GetSize().y - 1);
        }

        public static int Mod(int v, int base_v)
        {
            int result = v % base_v;
            if (result < 0) result += base_v; // So it can be always positive
            return result;
        }

        public static IComponent ComponentAt(Table table, (int x, int y) loc)
        {
            if (loc.y == 0)
            {
                if (loc.x == 0)
                {
                    return new TextLabel("MAX");
                }
                else if (loc.x == table.GetSize().x - 2)
                {
                    return new TextLabel("");
                }
                else if (loc.x == table.GetSize().x - 1)
                {
                    return new TextLabel("");
                }
            }
            if (loc.x == 0) return new TextLabel("");
            switch (Mod(loc.x - 3, 3))
            {
                case 0:
                    return new SimplexTableauVariableField();
                case 1:
                    return new TextLabel("+");
                case 2:
                    int idx = (loc.x - 5) / 3 + Const.DISPLAY_INDEX_OFFSET;
                    return new TextLabel($"x_{idx}");
                default:
                    throw new System.Exception("Unreachable code reached in StructuredInputTable.ComponentAt");
            }
            // if (loc.x == 1 || loc.x - 4 % 3 == 0)
            // {
            //     return new SimplexTableauVariableField();
            // }
            // if (loc.x - 3 % 3 == 0)
            // {
            //     return new TextLabel("+");
            // }
            // if (loc.x - 5 % 3 == 0)
            // {
            //     int idx = (loc.x - 5) / 3 + Const.DISPLAY_INDEX_OFFSET;
            //     return new TextLabel($"x_{idx}");
            // }
        }

        protected override Table InnerConstructor()
        {

            //  (MAX|"") [field] [var] ([+] [field] [var]))* (button|"") (RHS_value|"") // Show MAX on top and not the rhs value input and not the comparsion sign
            // Table table = new Table((5, 2)).WithComponentConstructor(
            //     ComponentAt
            // );
            Table table = new Table((1, 1));
            table.AddColumn(3);
            table.RemoveColumn(0); // The un-specified size row
            table.AddColumn(); // field
            table.AddColumn(3); // var
            table.AddColumn(2); // button
            table.AddColumn(); // RHS value
            table.WithComponentConstructor(
                ComponentAt
            );
            return table;
        }

        public StructuredInputTable() : base() { }

        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            throw new System.InvalidOperationException();
        }

        public override void InsertColumn(int idx, SplitAmount amount = null)
        {
            throw new System.InvalidOperationException();
        }

        public override void AddColumn(SplitAmount amount = null)
        {
            int cur_v = GetSize().x - 1 + Const.DISPLAY_INDEX_OFFSET;
            string text = $"x_{cur_v}";
            int idx = inner.GetSize().x - 2;
            inner.InsertColumn(idx, text.Length); // var
            inner.InsertColumn(idx, amount); // input field
            inner.InsertColumn(idx, 1); // plus sign
            for (int y = 0; y < inner.GetSize().y; y++)
            {
                for (int x = idx; x < idx + 3; x++)
                {
                    inner[x, y] = ComponentAt(inner, (x, y));
                }
            }
        }
        
        public override void AddRow(SplitAmount amount = null)
        {
            int idx = inner.GetSize().y;
            inner.AddRow(amount);
            for (int x = 0; x < inner.GetSize().x; x++)
            {
                inner[x, idx] = ComponentAt(inner, (x, idx));
            }
        }

        public override void RemoveColumn(int idx)
        {
            idx = inner.GetSize().x - 5;
            for (int i = 0; i < 3; i++)
                inner.RemoveColumn(idx);
        }

        public override void RemoveRow(int idx)
        {
            idx = inner.GetSize().y - 1;
            inner.RemoveRow(idx);
        }

        public (ExprResult, EqResult[]) Extract()
        {
            ExprResult comparsion = null;
            List<EqResult> expressions = new List<EqResult>();
            for (int y = 0; y < inner.GetSize().y; y++)
            {
                Term[] terms = new Term[(inner.GetSize().x - 5) / 3 + 1];
                for (int x = 1; x < inner.GetSize().x - 2; x += 3)
                {
                    SimplexTableauVariableField field = inner[x, y] as SimplexTableauVariableField;
                    TextLabel varLabel = inner[x + 1, y] as TextLabel;
                    terms[(x - 1) / 3] = new Term(Fraction.Parse(field.content), varLabel.text);
                }
                if (y == 0)
                {
                    comparsion = new ExprResult(terms);
                    continue;
                }
                ComparisionStateButton compButton = inner[inner.GetSize().x - 2, y] as ComparisionStateButton;
                SimplexTableauVariableField rhsField = inner[inner.GetSize().x - 1, y] as SimplexTableauVariableField;
                ExprResult expr = new ExprResult(terms) - (ExprResult)(Term)Fraction.Parse(rhsField.content);
                expressions.Add(new EqResult(expr, compButton.store.atom));
            }
            if (comparsion == null)
                throw new System.Exception("Unreachable code reached in StructuredInputTable.Extract");
            return (comparsion, expressions.ToArray());
        }
    }
}