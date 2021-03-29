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
    }
}