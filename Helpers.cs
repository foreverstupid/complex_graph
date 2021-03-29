using System;
using System.Numerics;

namespace ComplexGraph
{
    /// <summary>
    /// Help extension methods.
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Crops the given number if it is out of the range.
        /// </summary>
        public static bool Contains(this Range range, int n)
        {
            return
                range.Start.Value <= n &&
                n < range.End.Value;
        }

        /// <summary>
        /// Cehcks whether the given complex number is in a rectangle of the
        /// complex plane.
        /// </summary>
        /// <param name="point">Testing point.</param>
        /// <param name="leftBottom">Left bottom corner of a rectangle.</param>
        /// <param name="rightTop">Right top corner of a rectangle.</param>
        /// <param name="margin"Border margin as a ratio of rectangle sizes.</param>
        public static bool InRectangle(
            this Complex point,
            Complex leftBottom,
            Complex rightTop,
            double margin = 0.0)
        {
            double rMargin = margin * (rightTop.Real - leftBottom.Real);
            double iMargin = margin * (rightTop.Imaginary - leftBottom.Imaginary);

            if (rMargin < -1e-12 || iMargin < -1e-12)
            {
                throw new ArgumentException("Defined parameters are incorrect");
            }

            if (leftBottom.Real > rightTop.Real - margin ||
                leftBottom.Imaginary > rightTop.Imaginary - margin)
            {
                throw new ArgumentException("Defined rectangle is incorrect");
            }

            return
                leftBottom.Real + rMargin <= point.Real &&
                point.Real <= rightTop.Real - rMargin &&
                leftBottom.Imaginary + iMargin <= point.Imaginary &&
                point.Imaginary <= rightTop.Imaginary - iMargin;
        }

        /// <summary>
        /// Converts the given pattern to the function name.
        /// </summary>
        public static FunctionName ToName(this string pattern)
            => new FunctionName(pattern);
    }
}