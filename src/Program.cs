using System.Globalization;
using System.Linq;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using CommandLine;
using ComplexGraph.Verbs;

namespace ComplexGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<FunctionVerb>(args)
                .WithParsed<FunctionVerb>(v => v.Run())
                .WithNotParsed(_ => OldVersion(args));
        }

        private static void OldVersion(string[] args)
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
                        double.Parse(args[1], CultureInfo.InvariantCulture),
                        double.Parse(args[2], CultureInfo.InvariantCulture),
                        int.Parse(args[3]));

                    return;
                }

                if (args[0] == "--draw-exps" && args.Length > 3)
                {
                    DrawExps(
                        double.Parse(args[1], CultureInfo.InvariantCulture),
                        double.Parse(args[2], CultureInfo.InvariantCulture),
                        int.Parse(args[3]));

                    return;
                }
            }
        }

        private static void DrawExamples()
        {
            double tickStep = 1;
            double d = Math.PI;
            var area = new Area(new Complex(-d, -d), new Complex(d, d));
            var identity = Function.Identity;

            var funcs = new[]
            {
                ("sqrt", identity.RightCompose("sqrt(#)", Complex.Sqrt)),
                ("exp", identity.RightCompose("exp(#)", Complex.Exp)),
                ("ln", identity.RightCompose("ln #", Complex.Log)),
                ("sin", identity.RightCompose("sin #", Complex.Sin)),
                ("cos", identity.RightCompose("cos #", Complex.Cos)),
                ("tan", identity.RightCompose("tan #", Complex.Tan)),
            };

            var exDir = Path.Combine("..", "examples");
            foreach (var (fileName, func) in funcs)
            {
                using var plot = GetPlot();
                Draw(identity, area, plot.Canvas, plot.PreimageMask, tickStep);
                Draw(func, area, plot.Canvas, plot.ImageMask, tickStep, 16000, 16000);
                plot.Canvas.Save(Path.Combine(exDir, $"{fileName}.png"));
            }
        }

        private static void DrawPows(double start, double step, int count)
        {
            double tickStep = 0.5;
            var area = new Area(
                new Complex(-Math.PI, -Math.PI),
                new Complex(Math.PI, Math.PI));

            var identity = Function.Identity;

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
                Draw(identity, area, plot.Canvas, plot.PreimageMask, tickStep);
                Draw(func, area, plot.Canvas, plot.ImageMask, tickStep, 10000, 10000);
                plot.Canvas.Save(Path.Combine(powsDir, $"pow{i}.png"));
            }
        }

        private static void DrawExps(double start, double step, int count)
        {
            var sizes = Enumerable
                .Range(0, count)
                .Select(i => start + i * step);

            var tickSteps = new[]
            {
                0.01, 0.02, 0.05, 0.075,
                0.1,  0.2,  0.5,  0.75,
                1,    2,    5,    7.5
            };

            string expDir = "exps";

            if (Directory.Exists(expDir))
            {
                Directory.Delete(expDir, true);
            }

            Directory.CreateDirectory(expDir);
            foreach (var (size, i) in sizes.Select((t, i) => (t, i)))
            {
                Console.WriteLine($"Drawing size [{i}]: {size}...");
                var tickStep = tickSteps[^1];
                for (int j = 1; j < tickSteps.Length; j++)
                {
                    if (tickSteps[j] > size / 7)
                    {
                        tickStep = tickSteps[j];
                        break;
                    }
                }

                var area = new Area(
                    new Complex(-size, -size),
                    new Complex(size, size));

                var identity = Function.Identity;
                var func = identity.RightCompose($"exp(#)", Complex.Exp);
                using var plot = GetPlot();
                Draw(identity, area, plot.Canvas, plot.PreimageMask, tickStep);
                Draw(func, area, plot.Canvas, plot.ImageMask, tickStep, 8000, 8000);
                plot.Canvas.Save(Path.Combine(expDir, $"exp{i}.png"));
            }
        }

        private static void Draw(
            Function func,
            Area preimage,
            Bitmap plot,
            Rectangle mask,
            double tickStep,
            int? xCount = null,
            int? yCount = null)
        {
            var scan = plot.LockBits(mask, ImageLockMode.ReadWrite, Image.Format);
            var holder = new Image(scan);

            func.DrawTo(preimage, holder, xCount, yCount);
            holder.CopyToBitmapScan(scan);
            plot.UnlockBits(scan);

            plot.DrawCoordinateAxes(mask, preimage, tickStep, tickStep);
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
