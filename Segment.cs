using System;

namespace ComplexGraph
{
    /// <summary>
    /// Represent a segment of some real value (boundaries are included).
    /// </summary>
    internal struct Segment
    {
        public Segment(double min, double max)
        {
            if (!double.IsFinite(min))
            {
                throw new NotFiniteNumberException(nameof(min), min);
            }

            if (!double.IsFinite(max))
            {
                throw new NotFiniteNumberException(nameof(max), max);
            }

            if (min > max)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(min),
                    min,
                    $"Should be not greater than {nameof(max)} = {max}");
            }

            Min = min;
            Max = max;
        }

        /// <summary>
        /// Gets the minimal inclusive boundary.
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Gets the maximum inclusive boundary.
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Returns the length of a segment.
        /// </summary>
        public double Length()
            => Max - Min;
    }
}