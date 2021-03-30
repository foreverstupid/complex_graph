using System.Linq;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;

namespace ComplexGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "--draw-examples")
                {
                    DrawExamples();
                    return;
                }

                if (args[0] == "--draw-pows" && args.Length > 3)
                {
                    DrawPows(
                        double.Parse(args[1]),
                        double.Parse(args[2]),
                        int.Parse(args[3]));

                    return;
                }
            }

            using var plot = GetPlot();

            double size = 1.0;
            double tickStep = 0.2;
            var area = new Area(
                new Complex(-size, -size),
                new Complex(size, size));

            var identity = Function.Identity(area);
            var func = identity.RightCompose("#^2", c => c * c);

            Draw(identity, plot.Canvas, plot.PreimageMask, tickStep);
            Draw(func, plot.Canvas, plot.ImageMask, tickStep, 8000, 8000);
            plot.Canvas.Save("plot.png");
        }

        private static void DrawExamples()
        {
            double tickStep = 0.2;
            var area = new Area(new Complex(-1, -1), new Complex(1, 1));
            var identity = Function.Identity(area);

            var funcs = new[]
            {
                ("exp", identity.RightCompose("exp(#)", Complex.Exp)),
                ("ln", identity.RightCompose("ln #", Complex.Log)),
                ("sin", identity.RightCompose("sin #", Complex.Sin)),
                ("sqrt", identity.RightCompose("sqrt(#)", Complex.Sqrt)),
            };

            var exDir = Path.Combine("..", "examples");
            foreach (var (fileName, func) in funcs)
            {
                using var plot = GetPlot();
                Draw(identity, plot.Canvas, plot.PreimageMask, tickStep);
                Draw(func, plot.Canvas, plot.ImageMask, tickStep, 8000, 8000);
                plot.Canvas.Save(Path.Combine(exDir, $"{fileName}.png"));
            }
        }

        private static void DrawPows(double start, double step, int count)
        {
            double tickStep = 0.5;
            var area = new Area(
                new Complex(-Math.PI, -Math.PI),
                new Complex(Math.PI, Math.PI));

            var identity = Function.Identity(area);

            var pows = Enumerable.Range(0, count)
                .Select(i => start + step * i);

            string powsDir = "pows";
            if (Directory.Exists(powsDir))
            {
                Directory.Delete(powsDir, true);
            }

            Directory.CreateDirectory(powsDir);
            foreach (var (p, i) in pows.Select((t, i) => (t, i)))
            {
                Console.WriteLine($"Drawing power [{i}]: {p}...");
                var func = identity.RightCompose($"#^{p:0.##}", c => Complex.Pow(c, p));
                using var plot = GetPlot();
                Draw(identity, plot.Canvas, plot.PreimageMask, tickStep);
                Draw(func, plot.Canvas, plot.ImageMask, tickStep, 4000, 4000);
                plot.Canvas.Save(Path.Combine(powsDir, $"pow{i}.png"));
            }
        }

        private static void Draw(
            Function func,
            Bitmap plot,
            Rectangle mask,
            double tickStep,
            int? xCount = null,
            int? yCount = null)
        {
            var scan = plot.LockBits(mask, ImageLockMode.ReadWrite, Image.Format);
            var holder = new Image(scan);

            func.DrawTo(holder, xCount, yCount);
            holder.CopyToBitmapScan(scan);
            plot.UnlockBits(scan);

            plot.DrawCoordinateAxes(mask, func.Preimage, tickStep, tickStep);
            plot.DrawFuncName(mask, func.Name.Value);
        }

        private static Plot GetPlot()
        {
            int margin = 10;
            int spaceBetween = 100;
            int areaWidth = 500;
            int areaHeight = 500;
            var background = new SolidBrush(Color.LightGray);

            var bitmap = new Bitmap(
                4 * margin + spaceBetween + 2 * areaWidth,
                2 * margin + areaHeight);

            var preimageMask = new Rectangle(margin, margin, areaWidth, areaHeight);
            var imageMask = new Rectangle(
                3 * margin + spaceBetween + areaWidth,
                margin,
                areaWidth,
                areaHeight);

            using var graph = Graphics.FromImage(bitmap);
            graph.FillRectangle(background, 0, 0, bitmap.Width, bitmap.Height);

            return new Plot(bitmap, preimageMask, imageMask);
        }
    }
}
