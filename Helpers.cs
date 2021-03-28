using System;

namespace ComplexGraph
{
    /// <summary>
    /// Help extension methods.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Returns the length of the double-valued range.
        /// </summary>
        public static double Length(this (double Min, double Max) range)
        {
            if (range.Max < range.Min)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(range),
                    "Max cannot be less thatn min");
            }

            return range.Max - range.Min;
        }
    }
}