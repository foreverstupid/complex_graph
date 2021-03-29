using System.Security.AccessControl;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ComplexGraph
{
    public static class Drawer
    {
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
        private const int MeshCount = 10;

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

            // set steps for point color changes
            double realStep = func.Preimage.Width / (realGrid + 1);
            double imaginaryStep = func.Preimage.Height / (imaginaryGrid + 1);
            double huePerStep = HueRange.Length() / (realGrid + 1);
            double lightPerStep = LightnessRange.Length() / (imaginaryGrid + 1);

            // configure the mesh
            int meshStep = realGrid / (MeshCount + 1);
            int meshThick = (int)(realGrid * MeshThick);

            Parallel.For(0, imaginaryGrid, (j, ctxt) =>
            {
                int tmp = j % meshStep;
                bool isMesh =
                    (meshStep - meshThick) <= tmp &&
                    tmp < meshStep;

                var hsl = new HSL
                {
                    Hue = HueRange.Min,
                    Lightness = LightnessRange.Min + j * lightPerStep,
                };

                var point = new Complex(
                    func.Preimage.LeftBottom.Real,
                    func.Preimage.LeftBottom.Imaginary + j * imaginaryStep);

                for (int i = 0; i < realGrid; i++)
                {
                    var value = func[point];
                    bool isInsideArea = TryGetPlotPosition(
                        value,
                        func.Preimage,
                        plot.Width,
                        plot.Height,
                        out var pos);

                    if (isInsideArea)
                    {
                        int hlp = i % meshStep;
                        bool drawMesh =
                            isMesh ||
                            meshStep - meshThick <= hlp && hlp < meshStep;

                        if (drawMesh)
                        {
                            hsl.Saturation = MeshSaturation;
                        }

                        plot.SetPixel(pos.X, pos.Y, hsl.AsColor());
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
            return
                0 <= x && x < width &&
                0 <= y && y < height;
        }
    }
}