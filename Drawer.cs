using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ComplexGraph
{
    public static class Drawer
    {
        private static readonly Segment HueRange = new(0.0, 0.95);
        private static readonly Segment LightnessRange = new(0.05, 0.9);

        /// <summary>
        /// Draws the given function action on the plot.
        /// </summary>
        /// <param name="func">Drawing function.</param>
        /// <param name="plot">Image for drawing plot into.</param>
        /// <param name="realGridCount">The count of grid along real axis.</param>
        /// <param name="imaginaryGridCount">The count of grid along imaginary axis</param>
        public static void DrawTo(
            this Function func,
            Image plot,
            int? realGridCount = null,
            int? imaginaryGridCount = null)
        {
            int realGrid = realGridCount ?? plot.Width;
            int imaginaryGrid = imaginaryGridCount ?? plot.Height;

            double realStep = func.Preimage.Width / (realGrid + 1);
            double imaginaryStep = func.Preimage.Height / (imaginaryGrid + 1);

            double huePerPixel = HueRange.Length() / (realGrid + 1);
            double lightPerPixel = LightnessRange.Length() / (imaginaryGrid + 1);

            Parallel.For(0, imaginaryGrid, (j, ctxt) =>
            {
                var hsl = new HSL
                {
                    Hue = HueRange.Min,
                    Lightness = LightnessRange.Min + j * lightPerPixel,
                };

                var point = new Complex(
                    func.Preimage.LeftBottom.Real,
                    func.Preimage.LeftBottom.Imaginary + j * imaginaryStep);

                for (int _ = 0; _ < realGrid; _++)
                {
                    var value = func[point];
                    bool insideArea = TryGetPlotPosition(
                        value,
                        func.Preimage,
                        plot.Width,
                        plot.Height,
                        out var pos);

                    if (insideArea)
                    {
                        plot.SetPixel(pos.X, pos.Y, hsl.AsColor());
                    }

                    hsl.Hue += huePerPixel;
                    point = new Complex(point.Real + realStep, point.Imaginary);
                }
            });
        }

        /// <summary>
        /// Draws coordinate system on the plot.
        /// </summary>
        /// <param name="plot">Bitmap for drawing.</param>
        /// <param name="mask">Mask that defines available area of drawing.</param>
        /// <param name="area">Complex plane area whose coordinate system
        /// is drawing.</param>
        /// <param name="coordinateOrigin">The coordinate system origin.</param>
        public static void DrawCoordinateAxes(
            this Bitmap plot,
            Rectangle mask,
            Area area,
            Complex? coordinateOrigin = null)
        {
            var center = 0.5 * (area.LeftBottom + area.RightTop);
            var origin = coordinateOrigin ?? center;
            if (coordinateOrigin.HasValue &&
                !coordinateOrigin.Value.InRectangle(area.LeftBottom, area.RightTop, 0.1))
            {
                throw new ArgumentException("Defined coordinate origin is invalid");
            }

            if (!TryGetPlotPosition(
                    origin, area,
                    mask.Width, mask.Height,
                    out var pos))
            {
                throw new InvalidOperationException(
                    "Coordinate origin is drawn outside of the plot");
            }

            var (xPos, yPos) = pos;
            using var brush = new SolidBrush(Color.Black);
            using var pen = new Pen(brush, 3.0f);
            using var graph = Graphics.FromImage(plot);

            // imaginary axis
            graph.DrawLine(
                pen,
                xPos + mask.Left, mask.Top,
                xPos + mask.Left, mask.Bottom);

            // real axis
            graph.DrawLine(
                pen,
                mask.Left, yPos + mask.Top,
                mask.Right, yPos + mask.Top);

            using var font = new Font(new FontFamily("Times New Roman"), 12);
            graph.DrawString(
                $"Re = {origin.Real}", font, brush,
                xPos + mask.Left + 10, mask.Top + 10);

            graph.DrawString(
                $"Im = {origin.Imaginary}", font, brush,
                mask.Right - 50, yPos + 10);
        }

        /// <summary>
        /// Draws the function name on the plot.
        /// </summary>
        /// <param name="plot">Bitmap for drawing.</param>
        /// <param name="mask">Mask that defines available area of drawing.</param>
        /// <param name"name">Drawing function name.</param>
        public static void DrawFuncName(
            this Bitmap plot,
            Rectangle mask,
            string name)
        {
            using var brush = new SolidBrush(Color.Black);
            using var background = new SolidBrush(Color.White);
            using var margin = new Pen(new SolidBrush(Color.Black), 4);
            using var font = new Font(new FontFamily("Times New Roman"), 12, FontStyle.Italic);
            using var graphics = Graphics.FromImage(plot);

            var textSize = graphics.MeasureString(name, font);
            var point = new PointF(mask.Left + 5, mask.Top + 5);

            graphics.DrawRectangle(
                margin,
                point.X - 2,
                point.Y - 2,
                textSize.Width + 4,
                textSize.Height + 4);

            graphics.FillRectangle(background, new RectangleF(point, textSize));
            graphics.DrawString(name, font, brush, point);
        }

        private static bool TryGetPlotPosition(
            Complex c,
            Area area,
            int width,
            int height,
            out (int X, int Y) pos)
        {
            int x = (int)(
                (c.Real - area.LeftBottom.Real) /
                area.Width *
                width);

            int y = (int)(
                (c.Imaginary - area.LeftBottom.Imaginary) /
                area.Height *
                height);

            pos = (x, height - 1 - y);
            return (0..width).Contains(x) && (0..height).Contains(y);
        }
    }
}