using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ComplexGraph
{
    public static class Drawer
    {
        /// <summary>
        /// Coordinate line thickness relative to the plot area size.
        /// </summary>
        private const float CoordLineThick = 0.004f;

        /// <summary>
        /// Coordinate axes tick size relative to the plot area.
        /// </summary>
        private const float TickSize = 5 * CoordLineThick;

        /// <summary>
        /// Label size relative to the plot area size.
        /// </summary>
        private const float FontSize = 6 * CoordLineThick;

        /// <summary>
        /// Saturation of the color of points on the mesh.
        /// </summary>
        private const double MeshSaturation = 1.0;

        /// <summary>
        /// Saturation of the color of points out of the mesh.
        /// </summary>
        private const double DefaultSaturation = 0.5;

        /// <summary>
        /// The count of mesh step along the axis (similar for Re and Im).
        /// </summary>
        private const int MeshCount = 11;

        /// <summary>
        /// The mesh lines relative thickness (similar for Re and Im).
        /// </summary>
        private const double MeshThick = 4e-3;

        /// <summary>
        /// Range of using hue (changes along Re).
        /// </summary>
        private static readonly Segment HueRange = new(0.0, 0.95);

        /// <summary>
        /// Range of using lightness (changes along Im).
        /// </summary>
        /// <returns></returns>
        private static readonly Segment LightnessRange = new(0.05, 0.9);

        /// <summary>
        /// Draws the given function action on the plot.
        /// </summary>
        /// <param name="func">Drawing function.</param>
        /// <param name="preimage">The preimage area of the function action.</param>
        /// <param name="plot">Container for drawing plot into.</param>
        /// <param name="realGridCount">The count of grid along real axis.</param>
        /// <param name="imaginaryGridCount">The count of grid along imaginary axis</param>
        public static void DrawTo(
            this Function func,
            Area preimage,
            Image plot,
            int? realGridCount = null,
            int? imaginaryGridCount = null)
        {
            int realGrid = realGridCount ?? plot.Width;
            int imaginaryGrid = imaginaryGridCount ?? plot.Height;

            // set steps for point color changes
            double realStep = preimage.Width / (realGrid + 1);
            double imaginaryStep = preimage.Height / (imaginaryGrid + 1);
            double huePerStep = HueRange.Length() / (realGrid + 1);
            double lightPerStep = LightnessRange.Length() / (imaginaryGrid + 1);

            // configure the mesh
            double meshStepX = preimage.Width / (MeshCount + 1);
            double meshStepY = preimage.Height / (MeshCount + 1);
            double meshThick =
                Math.Max(preimage.Width, preimage.Height) *
                MeshThick;

            Parallel.For(0, imaginaryGrid, (j, ctxt) =>
            {
                var hsl = new HSL
                {
                    Hue = HueRange.Min,
                    Lightness = LightnessRange.Min + j * lightPerStep,
                };

                var point = new Complex(
                    preimage.LeftBottom.Real,
                    preimage.LeftBottom.Imaginary + j * imaginaryStep);

                bool isImMesh = IsOnMesh(point.Imaginary, meshStepY, meshThick);
                for (int i = 0; i < realGrid; i++)
                {
                    var value = func[point];
                    bool isInsideArea = TryGetPlotPosition(
                        value,
                        preimage,
                        plot.Width,
                        plot.Height,
                        out var pos);

                    if (isInsideArea)
                    {
                        bool drawMesh =
                            isImMesh ||
                            IsOnMesh(point.Real, meshStepX, meshThick);

                        if (drawMesh)
                        {
                            hsl.Saturation = MeshSaturation;
                        }

                        plot.SetPixel(pos.X, pos.Y, hsl.AsColor(), point);
                        hsl.Saturation = DefaultSaturation;
                    }

                    hsl.Hue += huePerStep;
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
            double xStep = 0.5,
            double yStep = 0.5,
            Complex? coordinateOrigin = null)
        {
            var center = 0.5 * (area.LeftBottom + area.RightTop);
            var origin = coordinateOrigin ?? center;
            if (coordinateOrigin.HasValue &&
                !area.Contains(coordinateOrigin.Value, 0.1))
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
            var size = Math.Max(mask.Width, mask.Height);
            var lineThick = size * CoordLineThick;
            var fontSize = size * FontSize;

            using var brush = new SolidBrush(Color.Black);
            using var pen = new Pen(brush, lineThick);
            using var graphics = Graphics.FromImage(plot);

            // real axis
            graphics.DrawArrow(
                pen,
                mask.Left, yPos + mask.Top,
                mask.Right, yPos + mask.Top,
                fontSize);

            // imaginary axis
            graphics.DrawArrow(
                pen,
                xPos + mask.Left, mask.Bottom,
                xPos + mask.Left, mask.Top,
                fontSize);

            using var font = new Font(new FontFamily("Times New Roman"), fontSize);
            int tickSize = (int)(size * TickSize);

            DrawReTicks();
            DrawImTicks();

            var reLabel = "Re z";
            var imLabel = "Im z";

            var reLabelSize = graphics.MeasureString(reLabel, font);
            var imLabelSize = graphics.MeasureString(imLabel, font);

            graphics.DrawString(
                reLabel, font, brush,
                mask.Right - reLabelSize.Width,
                yPos + reLabelSize.Height);

            graphics.DrawString(
                imLabel, font, brush,
                xPos + mask.Left - imLabelSize.Width - fontSize,
                mask.Top);

            void DrawReTicks()
            {
                var ticks = new List<double>();
                var t = origin.Real + xStep;
                while (t < area.RightTop.Real)
                {
                    ticks.Add(t);
                    t += xStep;
                }

                t = origin.Real - xStep;
                while (t > area.LeftBottom.Real)
                {
                    ticks.Add(t);
                    t -= xStep;
                }

                foreach (var tick in ticks)
                {
                    var p = new Complex(tick, origin.Imaginary);
                    TryGetPlotPosition(p, area, mask.Width, mask.Height, out var tickPos);
                    graphics.DrawLine(
                        pen,
                        mask.Left + tickPos.X, mask.Top + tickPos.Y,
                        mask.Left + tickPos.X, mask.Top + tickPos.Y - tickSize);

                    var tLabel = $"{tick:0.##}";
                    var tSize = graphics.MeasureString(tLabel, font);
                    graphics.DrawString(
                        tLabel, font, brush,
                        mask.Left + tickPos.X - tSize.Width / 2,
                        mask.Top + tickPos.Y - tickSize - tSize.Height);
                }
            }

            void DrawImTicks()
            {
                var ticks = new List<double>();
                var t = origin.Imaginary + yStep;
                while (t < area.RightTop.Imaginary)
                {
                    ticks.Add(t);
                    t += yStep;
                }

                t = origin.Imaginary - yStep;
                while (t > area.LeftBottom.Imaginary)
                {
                    ticks.Add(t);
                    t -= yStep;
                }

                foreach (var tick in ticks)
                {
                    var p = new Complex(origin.Real, tick);
                    TryGetPlotPosition(p, area, mask.Width, mask.Height, out var tickPos);
                    graphics.DrawLine(
                        pen,
                        mask.Left + tickPos.X, mask.Top + tickPos.Y,
                        mask.Left + tickPos.X + tickSize, mask.Top + tickPos.Y);

                    var tLabel = $"{tick:0.##}";
                    var tSize = graphics.MeasureString(tLabel, font);
                    graphics.DrawString(
                        tLabel, font, brush,
                        mask.Left + tickPos.X + tickSize,
                        mask.Top + tickPos.Y - tSize.Height / 2);
                }
            }
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
            var fontSize = FontSize * Math.Max(mask.Width, mask.Height);

            using var brush = new SolidBrush(Color.Black);
            using var background = new SolidBrush(Color.White);
            using var margin = new Pen(new SolidBrush(Color.Black), 4);
            using var font = new Font(
                new FontFamily("Times New Roman"),
                fontSize,
                FontStyle.Italic);

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
            return
                0 <= x && x < width &&
                0 <= y && y < height;
        }

        private static bool IsOnMesh(
            double point,
            double meshStep,
            double meshThick)
        {
            var tmp = point % meshStep;
            if (tmp < 0)
            {
                tmp += meshStep;
            }

            return
                tmp > meshStep / 2 &&
                tmp <= meshStep / 2 + meshThick;
        }
    }
}