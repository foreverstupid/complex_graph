using System;
using System.Drawing;
using System.Numerics;

namespace ComplexGraph
{
    /// <summary>
    /// Represents a part of a complex plane whose points are colored in
    /// some way.
    /// </summary>
    public class Area
    {
        private static (double Min, double Max) HueRange = (0.0, 0.95);
        private static (double Min, double Max) LightnessRange = (0.05, 0.9);

        private readonly Complex leftBottom;
        private readonly Complex rightTop;

        public Area(Complex leftBottom, Complex rightTop)
        {
            this.leftBottom = leftBottom;
            this.rightTop = rightTop;
            if (rightTop.Imaginary <= leftBottom.Imaginary ||
                rightTop.Real <= leftBottom.Real)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rightTop),
                    $"Should have greater real and imaginary part than {nameof(leftBottom)} has");
            }
        }

        public Bitmap ToBitmap(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            double huePerPixel = HueRange.Length() / (width + 1);
            double lightPerPixel = LightnessRange.Length() / (height + 1);

            var hsl = new HSL() { Hue = HueRange.Min };
            for (int i = 0; i < width; i++)
            {
                hsl.Lightness = LightnessRange.Min;
                for (int j = 0; j < height; j++)
                {
                    bitmap.SetPixel(i, j, hsl.AsColor());
                    hsl.Lightness += lightPerPixel;
                }

                hsl.Hue += huePerPixel;
            }

            return bitmap;
        }
    }
}