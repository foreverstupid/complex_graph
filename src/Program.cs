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
                .ParseArguments<FunctionVerb, PowsVerb, ExpsVerb>(args)
                .WithParsed<FunctionVerb>(v => v.Run())
                .WithParsed<PowsVerb>(v => v.Run())
                .WithParsed<ExpsVerb>(v => v.Run())
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
