using System;
using System.IO;
using System.Numerics;
using CommandLine;

namespace ComplexGraph.Verbs
{
    [Verb("examples", HelpText = "Draws basic example functions")]
    class ExpamplesVerb : Verb
    {
        [Option(
            'a', "area-size",
            HelpText = "Widht and height of the centered squared area",
            Default = 2 * Math.PI)]
        public double Size { get; set; }

        [Option(
            't', "ticks-step",
            HelpText = "Ticks step for both axes",
            Default = 1)]
        public double TicksStep { get; set; }

        [Option(
            'd', "directory",
            HelpText = "Results directory name",
            Default = "examples")]
        public string Dir { get; set; } = "examples";

        public override void Run()
        {
            var area = new Area(
                new Complex(-Size/2, -Size/2),
                new Complex(Size/2, Size/2));

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

            if (Directory.Exists(Dir))
            {
                Directory.Delete(Dir, true);
            }

            Directory.CreateDirectory(Dir);
            foreach (var (fileName, func) in funcs)
            {
                Console.WriteLine($"Drawing {fileName}...");
                using var plot = GetPlot();
                Draw(identity, area, plot.Canvas, plot.PreimageMask, TicksStep);
                Draw(
                    func, area,
                    plot.Canvas, plot.ImageMask, TicksStep,
                    Quality, Quality);

                plot.Canvas.Save(Path.Combine(Dir, $"{fileName}.png"));
            }
        }
    }
}