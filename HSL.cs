using System;
using System.Drawing;

namespace ComplexGraph
{
    /// <summary>
    /// HSL representation of some color.
    /// </summary>
    public class HSL
    {
        private double hue;
        private double saturation = 1.0;
        private double lightness;

        /// <summary>
        /// Gets or sets the hue of the color. Should be in range [0; 1].
        /// </summary>
        public double Hue
        {
            get => hue;
            set
            {
                if (value > 1.0 || value < 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Hue),
                        "Should be in range [0; 1]");
                }

                hue = value;
            }
        }

        /// <summary>
        /// Gets or sets the saturation of the color. Should be in range [0; 1].
        /// </summary>
        public double Saturation
        {
            get => saturation;
            set
            {
                if (value > 1.0 || value < 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Saturation),
                        "Should be in range [0; 1]");
                }

                saturation = value;
            }
        }

        /// <summary>
        /// Gets or sets the lightness of the color. Should be in range [0; 1].
        /// </summary>
        public double Lightness
        {
            get => lightness;
            set
            {
                if (value > 1.0 || value < 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Lightness),
                        "Should be in range [0; 1]");
                }

                lightness = value;
            }
        }

        /// <summary>
        /// Returns the color as <see cref="Color"/>.
        /// </summary>
        /// <returns>An instance of <see cref="Color"/>, that represents
        /// this <see cref="HSL"/>.</returns>
        public Color AsColor()
        {
            var q =
                lightness < 0.5
                ? lightness * (saturation + 1.0)
                : lightness + saturation - (lightness * saturation);

            var p = 2.0 * lightness - q;
            var ts = new[] { hue + 1.0 / 3, hue, hue - 1.0 / 3 };
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] > 1.0)
                {
                    ts[i] -= 1.0;
                }

                if (ts[i] < 0.0)
                {
                    ts[i] += 1.0;
                }
            }

            var components = new double[3]; // R, G, B
            for (int i = 0; i < components.Length; i++)
            {
                components[i] = ts[i] switch
                {
                    var t when t < 1.0 / 6 => p + ((q - p) * 6.0 * t),
                    < 0.5 => q,
                    var t when t < 2.0 / 3 => p + ((q - p) * (2.0 / 3 - t) * 6.0),
                    _ => p,
                };
            }

            return Color.FromArgb(
                red: (int)(components[0] * 255),
                green: (int)(components[1] * 255),
                blue: (int)(components[2] * 255));
        }
    }
}