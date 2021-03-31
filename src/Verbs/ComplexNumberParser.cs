using System;
using System.Globalization;
using System.Numerics;

using static System.FormattableString;

namespace ComplexGraph.Verbs
{
    /// <summary>
    /// Converter for de/serializing complex number from/to string.
    /// </summary>
    static class ComplexNumberParser
    {
        private const double Eps = 1e-12;

        private const char ImgOne = 'i';
        private const char Separator = ',';
        private const char Start = '{';
        private const char End = '}';

        /// <summary>
        /// Tries to get complex number by its string representation, returning
        /// true in the case of success.
        /// </summary>
        public static bool TryParse(string str, out Complex number)
        {
            number = Complex.Zero;
            if (TryGetReal(str, out double re))
            {
                number = new Complex(re, 0.0);
                return true;
            }

            if (TryGetImaginary(str, out double im))
            {
                number = new Complex(0.0, im);
                return true;
            }

            if (str[0] == Start && str[^1] == End)
            {
                str = str[1..^1];
                var parts = str.Split(Separator);

                var reStr = parts[0];
                var imStr = parts[1];
                if (!TryGetReal(reStr, out re) ||
                    !TryGetReal(imStr, out im))
                {
                    return false;
                }

                number = new Complex(re, im);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Saves complex number into string. This operation is opposite
        /// to <see cref="TryParse"/>.
        /// </summary>
        public static string ToString(Complex c)
        {
            if (Math.Abs(c.Imaginary) < Eps)
            {
                return c.Real.ToString(CultureInfo.InvariantCulture);
            }

            if (Math.Abs(c.Real) < 1e-12)
            {
                return
                    Math.Abs(c.Imaginary - 1.0) < 1e-12
                    ? ImgOne.ToString()
                    : Invariant($"{c.Imaginary}{ImgOne}");
            }

            return Invariant(
                $"{Start}{c.Real}{Separator}{c.Imaginary}{ImgOne}{End}");
        }

        private static bool TryGetReal(string str, out double num)
            => double.TryParse(
                    str,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out num);

        private static bool TryGetImaginary(string str, out double num)
        {
            if (str[^1] != ImgOne)
            {
                num = 0.0;
                return false;
            }

            if (str.Length == 1)
            {
                num = 1.0;
                return true;
            }

            return TryGetReal(str[..^1], out num);
        }
    }
}