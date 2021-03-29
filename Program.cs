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

                if (args[0] == "--draw-pows")
                {
                    DrawPows();
                    return;
                }
            }

            using var plot = GetPlot();

            var area = new Area(
                new Complex(-Math.PI, -Math.PI),
                new Complex(Math.PI, Math.PI));

            var identity = Function.Identity(area);
            var func = identity.RightCompose("sqrt(#)", Complex.Sqrt);

            Draw(identity, plot.Canvas, plot.PreimageMask);
            Draw(func, plot.Canvas, plot.ImageMask, 8000, 8000);
            plot.Canvas.Save("plot.png");
        }

        private static void DrawExamples()
        {
            var area = new Area(new Complex(-1, -1), new Complex(1, 1));
            var identity = Function.Identity(area);

            var funcs = new[]
            {
                ("exp", identity.RightCompose("exp(#)", Complex.Exp)),
                ("ln", identity.RightCompose("ln #", Complex.Log)),
                ("sin", identity.RightCompose("sin #", Complex.Sin)),
                ("sqrt", identity.RightCompose("sqrt(#)", Complex.Sqrt)),
            };

            foreach (var (fileName, func) in funcs)
            {
                using var plot = GetPlot();
                Draw(identity, plot.Canvas, plot.PreimageMask);
                Draw(func, plot.Canvas, plot.ImageMask, 8000, 8000);
                plot.Canvas.Save(Path.Combine("examples", $"{fileName}.png"));
            }
        }

        private static void DrawPows()
        {
            var area = new Area(
                new Complex(-Math.PI, -Math.PI),
                new Complex(Math.PI, Math.PI));

            var identity = Function.Identity(area);

            var pows = Enumerable.Range(1, 100)
                .Reverse()
                .Select(i => 0.01 * i);

            string powsDir = "pows";
            if (Directory.Exists(powsDir))
            {
                Directory.Delete(powsDir, true);
            }

            Directory.CreateDirectory(powsDir);
            foreach (var (p, i) in pows.Select((t, i) => (t, i)))
            {
                Console.WriteLine($"Drawing power {p}...");
                var func = identity.RightCompose($"#^{p:0.##}", c => Complex.Pow(c, p));
                using var plot = GetPlot();
                Draw(identity, plot.Canvas, plot.PreimageMask);
                Draw(func, plot.Canvas, plot.ImageMask, 4000, 4000);
                plot.Canvas.Save(Path.Combine(powsDir, $"pow{i}.png"));
            }
        }

        private static void Draw(
            Function func,
            Bitmap plot,
            Rectangle mask,
            int? xCount = null,
            int? yCount = null)
        {
            var scan = plot.LockBits(mask, ImageLockMode.ReadWrite, Image.Format);
            var holder = new Image(scan);

            func.DrawTo(holder, xCount, yCount);
            holder.CopyToBitmapScan(scan);
            plot.UnlockBits(scan);

            plot.DrawCoordinateAxes(mask, func.Preimage);
            plot.DrawFuncName(mask, func.Name.Value);
        }

        private static Plot GetPlot()
        {
            int margin = 20;
            int spaceBetween = 200;
            int areaWidth = 1000;
            int areaHeight = 1000;
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
