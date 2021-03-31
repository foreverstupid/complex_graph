using System;
using System.IO;
using System.Linq;
using System.Numerics;
using CommandLine;

namespace ComplexGraph.Verbs
{
    [Verb("pows", HelpText = "Draws series of power function")]
    class PowsVerb : Verb
    {
        [Option(
            'o', "origin",
            HelpText = "Starting power",
            Default = 2.0)]
        public double Origin { get; set; }

        [Option(
            's', "step",
            HelpText = "Power changing step",
            Default = -0.1)]
        public double Step { get; set; }

        [Option(
            'c', "count",
            HelpText = "Series count",
            Default = 30)]
        public int Count { get; set; }

        [Option(
            'a', "area-size",
            HelpText = "Widht and height of the centered squared area",
            Default = 6)]
        public double Size { get; set; }

        [Option(
            't', "ticks-step",
            HelpText = "Ticks step for both axes",
            Default = 1)]
        public double TicksStep { get; set; }

        [Option(
            'd', "directory",
            HelpText = "Results directory name",
            Default = "pows")]
        public string Dir { get; set; } = "pows";

        public override void Run()
        {
            var ztype = Image.ZorderType.Real;
            var area = new Area(
                new Complex(-Size / 2, -Size / 2),
                new Complex(Size / 2, Size / 2));

            var identity = Function.Identity;

            var pows = Enumerable.Range(0, Count)
                .Select(i => Origin + Step * i);

            if (Directory.Exists(Dir))
            {
                Directory.Delete(Dir, true);
            }

            Directory.CreateDirectory(Dir);
            foreach (var (p, i) in pows.Select((t, i) => (t, i)))
            {
                Console.WriteLine($"Drawing power [{i}]: {p}...");
                var func = identity
                    .RightCompose($"#^{p:0.##}", c => Complex.Pow(c, p));

                using var plot = GetPlot();
                Draw(identity, area, plot.Canvas, plot.PreimageMask, TicksStep);
                Draw(
                    func, area,
                    plot.Canvas, plot.ImageMask, TicksStep,
                    Quality, Quality,
                    ztype);

                plot.Canvas.Save(Path.Combine(Dir, $"pow{i}.png"));
            }
        }
    }
}