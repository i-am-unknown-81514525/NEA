using ui.math;

namespace NEA.Math
{
    public static class TransitivityExt
    {
        public static Fraction Transitivity(this math_parser.math.Fraction src)
        {
            return new Fraction(src.Numerator, src.Denominator);
        }

        public static math_parser.math.Fraction Transitivity(this Fraction src)
        {
            return new math_parser.math.Fraction(src.Numerator, src.Denominator);
        }
    }
}