using System;
using System.Collections.Generic;
using math_parser.math;
using math_parser.tokenizer;
using ui.components;
using ui.components.chainExt;
using ui.fmt;
using ui.utils;
using ComparsionSymbolAtom = math_parser.atom.ComparsionSymbolAtom;

namespace NEA.components
{
    public class ComparsionStateStore : ComponentStore
    {
        public ComparsionSymbolAtom Atom;

        public ComparsionStateStore(ComparsionSymbolAtom atom)
        {
            Atom = atom;
        }
    }

    public class ComparisionStateButton : Button<ComparsionStateStore>
    {
        public override ComparsionStateStore ComponentStoreConstructor()
        {
            return new ComparsionStateStore(null);
        }

        public ComparisionStateButton(ComparsionSymbolAtom symbol = null)
        {
            Store.Atom = symbol ?? ComparsionSymbolAtom.Le;
            text = Store.Atom.Literal;
            OnClickHandler = (btn, loc) =>
            {
                btn.Store.Atom = (
                    new Dictionary<ComparsionSymbolAtom, ComparsionSymbolAtom>
                    {
                        { ComparsionSymbolAtom.Le, ComparsionSymbolAtom.Ge },
                        { ComparsionSymbolAtom.Ge, ComparsionSymbolAtom.Eq },
                        { ComparsionSymbolAtom.Eq, ComparsionSymbolAtom.Le },
                    }
                )[btn.Store.Atom];
                text = btn.Store.Atom.Literal;

            };
            foreground = ForegroundColorEnum.BLACK;
        }
    }

    public class StructuredInputTable : VirtualTable<Table>
    {
        public override (int x, int y) GetSize()
        {
            return ((Inner.GetSize().x - 5) / 3 + 1, Inner.GetSize().y - 1);
        }

        public static int Mod(int v, int baseV)
        {
            int result = v % baseV;
            if (result < 0) result += baseV; // So it can be always positive
            return result;
        }

        public static IComponent ComponentAt(Table table, (int x, int y) loc)
        {
            if (loc.y == 0)
            {
                if (loc.x == 0)
                {
                    return new TextLabel("MAX ");
                }

                if (loc.x == table.GetSize().x - 2)
                {
                    return new TextLabel("");
                }

                if (loc.x == table.GetSize().x - 1)
                {
                    return new TextLabel("");
                }
            }
            if (loc.x == 0) return new TextLabel("");
            if (loc.x == table.GetSize().x - 2)
            {
                return new ComparisionStateButton();
            }

            if (loc.x == table.GetSize().x - 1)
            {
                return new SimplexTableauValueField();
            }
            //  2          0      1     2     3
            //  (MAX|"") [field] [var] ([+] [field] [var]))* (button|"") (RHS_value|"") // Show MAX on top and not the rhs value input and not the comparsion sign
            switch (Mod(loc.x - 1, 3))
            {
                case 0:
                    return new SimplexTableauValueField();
                case 1:
                    int idx = (loc.x - 5) / 3 + Const.DisplayIndexOffset;
                    return new TextLabel($"x_{idx}");
                case 2:
                    return new TextLabel("+");
                default:
                    throw new Exception("Unreachable code reached in StructuredInputTable.ComponentAt");
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
            table.AddColumn(4);
            table.RemoveColumn(0); // The un-specified size row
            table.AddColumn(); // field
            table.AddColumn(3); // var
            table.AddColumn(4); // button
            table.AddColumn(); // RHS value
            table.AddRow(1);
            table.RemoveRow(0); // The un-specified size column
            table.AddRow(1);
            table.WithComponentConstructor(
                ComponentAt
            );
            return table;
        }

        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            throw new InvalidOperationException();
        }

        public override void InsertColumn(int idx, SplitAmount amount = null)
        {
            throw new InvalidOperationException();
        }

        public override void AddColumn(SplitAmount amount = null)
        {
            int curV = GetSize().x - 1 + Const.DisplayIndexOffset;
            string text = $"x_{curV}";
            int idx = Inner.GetSize().x - 2;
            Inner.InsertColumn(idx, text.Length); // var
            Inner.InsertColumn(idx, amount); // input field
            Inner.InsertColumn(idx, 3); // plus sign
            for (int y = 0; y < Inner.GetSize().y; y++)
            {
                for (int x = idx; x < idx + 3; x++)
                {
                    Inner[x, y] = ComponentAt(Inner, (x, y));
                }
            }
        }

        public override void AddRow(SplitAmount amount = null)
        {
            int idx = Inner.GetSize().y;
            Inner.AddRow(amount);
            for (int x = 0; x < Inner.GetSize().x; x++)
            {
                Inner[x, idx] = ComponentAt(Inner, (x, idx));
            }
        }

        public override void RemoveColumn(int idx)
        {
            idx = Inner.GetSize().x - 5;
            for (int i = 0; i < 3; i++)
                Inner.RemoveColumn(idx);
        }

        public override void RemoveRow(int idx)
        {
            idx = Inner.GetSize().y - 1;
            Inner.RemoveRow(idx);
        }

        public (ExprResult, EqResult[]) Extract()
        {
            ExprResult comparsion = null;
            List<EqResult> expressions = new List<EqResult>();
            for (int y = 0; y < Inner.GetSize().y; y++)
            {
                Term[] terms = new Term[(Inner.GetSize().x - 5) / 3 + 1];
                for (int x = 1; x < Inner.GetSize().x - 2; x += 3)
                {
                    SimplexTableauVariableField field = Inner[x, y] as SimplexTableauVariableField;
                    TextLabel varLabel = Inner[x + 1, y] as TextLabel;
                    terms[(x - 1) / 3] = new Term(Fraction.Parse(field.content), varLabel.text);
                }
                if (y == 0)
                {
                    comparsion = new ExprResult(terms);
                    continue;
                }
                ComparisionStateButton compButton = Inner[Inner.GetSize().x - 2, y] as ComparisionStateButton;
                SimplexTableauVariableField rhsField = Inner[Inner.GetSize().x - 1, y] as SimplexTableauVariableField;
                ExprResult expr = new ExprResult(terms) - (Term)Fraction.Parse(rhsField.content);
                expressions.Add(new EqResult(expr, compButton.Store.Atom));
            }
            if (comparsion == null)
                throw new Exception("Unreachable code reached in StructuredInputTable.Extract");
            return (comparsion, expressions.ToArray());
        }
    }
}