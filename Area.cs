using System;
using System.Drawing;
using System.Drawing.Text;
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
        private readonly Func<Complex, Complex> func;
        private readonly string funcName;

        public Area(
            Complex leftBottom,
            Complex rightTop,
            Func<Complex, Complex> func = null,
            string funcName = "z")
        {
            this.leftBottom = leftBottom;
            this.rightTop = rightTop;
            this.func = func ?? (Func<Complex, Complex>)(c => c);
            this.funcName = funcName;

            if (rightTop.Imaginary <= leftBottom.Imaginary ||
                rightTop.Real <= leftBottom.Real)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rightTop),
                    $"Should have greater real and imaginary part " +
                    $"than {nameof(leftBottom)} has");
            }
        }

        public Area Apply(Func<Complex, Complex> func, string funcName)
        {
            Func<Complex, Complex> newFunc = c => func(this.func(c));
            return new Area(
                this.leftBottom,
                this.rightTop,
                newFunc,
                $"{funcName} of {this.funcName}");
        }

        public Bitmap DrawTo(
            Bitmap bitmap,
            Rectangle mask,
            int? realGridCount = null,
            int? imaginaryGridCount = null)
        {
            int realGrid = realGridCount ?? mask.Width;
            int imaginaryGrid = imaginaryGridCount ?? mask.Height;

            double preimageWidth = rightTop.Real - leftBottom.Real;
            double preimageHeight = rightTop.Imaginary - leftBottom.Imaginary;
            double realStep = preimageWidth / (realGrid + 1);
            double imaginaryStep = preimageHeight / (imaginaryGrid + 1);

            double huePerPixel = HueRange.Length() / (realGrid + 1);
            double lightPerPixel = LightnessRange.Length() / (imaginaryGrid + 1);

            var hsl = new HSL() { Hue = HueRange.Min };
            var point = leftBottom;
            for (int i = 0; i < realGrid; i++)
            {
                hsl.Lightness = LightnessRange.Min;
                point = new Complex(point.Real, leftBottom.Imaginary);
                for (int j = 0; j < imaginaryGrid; j++)
                {
                    var value = this.func(point);
                    var (x, y) = GetPlotPosition(value, mask.Width, mask.Height);
                    bitmap.SetPixel(mask.Left + x, mask.Bottom - 1 - y, hsl.AsColor());
                    hsl.Lightness += lightPerPixel;
                    point = new Complex(point.Real, point.Imaginary + imaginaryStep);
                }

                hsl.Hue += huePerPixel;
                point = new Complex(point.Real + realStep, point.Imaginary);
            }

            DrawCoordinateAxes(bitmap, mask);
            DrawFuncName(bitmap, mask);
            return bitmap;
        }

        private void DrawCoordinateAxes(Bitmap plot, Rectangle mask)
        {
            double x = 0.5 * (rightTop.Real + leftBottom.Real);
            double y = 0.5 * (rightTop.Imaginary + leftBottom.Imaginary);

            if ((leftBottom.Real, rightTop.Real).Contains(0.0))
            {
                x = 0.0;
            }

            if ((leftBottom.Imaginary, rightTop.Imaginary).Contains(0.0))
            {
                y = 0.0;
            }

            var origin = new Complex(x, y);
            var (xPos, yPos) = GetPlotPosition(origin, mask.Width, mask.Height);

            var brush = new SolidBrush(Color.Black);
            var pen = new Pen(brush, 3.0f);
            using var graph = Graphics.FromImage(plot);

            graph.DrawLine(
                pen,
                xPos + mask.Left,
                mask.Top,
                xPos + mask.Left,
                mask.Bottom);

            graph.DrawLine(
                pen,
                mask.Left,
                mask.Bottom - 1 - yPos,
                mask.Right,
                mask.Bottom - 1 - yPos);

            var font = new Font(new FontFamily("Times New Roman"), 12);
            graph.DrawString($"Re = {x}", font, brush, xPos + mask.Left + 10, mask.Top + 10);
            graph.DrawString($"Im = {y}", font, brush, mask.Right - 50, mask.Bottom - 1 - yPos + 10);
        }

        private void DrawFuncName(Bitmap plot, Rectangle mask)
        {
            var brush = new SolidBrush(Color.Black);
            var background = new SolidBrush(Color.White);
            var margin = new Pen(new SolidBrush(Color.Black), 4);
            var font = new Font(new FontFamily("Times New Roman"), 12, FontStyle.Italic);

            using var graphics = Graphics.FromImage(plot);
            var textSize = graphics.MeasureString(funcName, font);
            var point = new PointF(mask.Left + 5, mask.Top + 5);

            graphics.DrawRectangle(
                margin,
                point.X - 2,
                point.Y - 2,
                textSize.Width + 4,
                textSize.Height + 4);

            graphics.FillRectangle(background, new RectangleF(point, textSize));
            graphics.DrawString(funcName, font, brush, point);
        }

        private (int, int) GetPlotPosition(Complex c, int plotWidth, int plotHeight)
        {
            int x = (int)((c.Real - leftBottom.Real) / (rightTop.Real - leftBottom.Real) * plotWidth);
            int y = (int)((c.Imaginary - leftBottom.Imaginary) / (rightTop.Imaginary - leftBottom.Imaginary) * plotHeight);
            if (x < 0)
            {
                x = 0;
            }

            if (x >= plotWidth)
            {
                x = plotWidth - 1;
            }

            if (y < 0)
            {
                y = 0;
            }

            if (y >= plotHeight)
            {
                y = plotHeight - 1;
            }

            return (x, y);
        }
    }
}