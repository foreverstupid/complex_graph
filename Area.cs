using System;
using System.Numerics;

namespace ComplexGraph
{
    /// <summary>
    /// Represents a part of a complex plane.
    /// </summary>
    public struct Area
    {
        public Area(Complex leftBottom, Complex rightTop)
        {
            if (rightTop.Imaginary <= leftBottom.Imaginary ||
                rightTop.Real <= leftBottom.Real)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rightTop),
                    $"Should have greater real and imaginary part " +
                    $"than {nameof(leftBottom)} has");
            }

            LeftBottom = leftBottom;
            RightTop = rightTop;
        }

        public Complex LeftBottom { get; }

        public Complex RightTop { get; }

        public double Width =>
            RightTop.Real - LeftBottom.Real;

        public double Height =>
            RightTop.Imaginary - LeftBottom.Imaginary;

        /// <summary>
        /// Cehcks whether the area contains the given point.
        /// </summary>
        /// <param name="point">Testing point.</param>
        /// <param name="margin">Border margin as a ratio of area sizes.
        /// The point should be inside the area and far from the boundary
        /// by this margin.</param>
        public bool Contains(Complex point, double margin = 0.0)
        {
            if (margin < -1e-12 || margin >= 0.5)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(margin),
                    margin,
                    "Should be in range [0; 0.5)");
            }

            double rMargin = margin * (RightTop.Real - LeftBottom.Real);
            double iMargin = margin * (RightTop.Imaginary - LeftBottom.Imaginary);

            return
                LeftBottom.Real + rMargin <= point.Real &&
                point.Real <= RightTop.Real - rMargin &&
                LeftBottom.Imaginary + iMargin <= point.Imaginary &&
                point.Imaginary <= RightTop.Imaginary - iMargin;
        }
    }
}