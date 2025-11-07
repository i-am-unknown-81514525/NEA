using ui;
using math_parser;

namespace NEA.Math
{
    public static class TransitivityExt
    {
        public static ui.math.Fraction Transitivity(this math_parser.math.Fraction src)
        {
            return new ui.math.Fraction(src.numerator, src.denominator);
        }

        public static math_parser.math.Fraction Transitivity(this ui.math.Fraction src)
        {
            return new math_parser.math.Fraction(src.numerator, src.denominator);
        }
    }
}