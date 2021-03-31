using System;
using System.IO;
using System.Linq;
using System.Numerics;
using CommandLine;

namespace ComplexGraph.Verbs
{
    [Verb("exps", HelpText = "Draws series of exponent function on different scale")]
    class ExpsVerb : Verb
    {
        [Option(
            'o', "origin",
            HelpText = "Starting squared centered area size",
            Default = 2 * Math.PI)]
        public double Origin { get; set; }

        [Option(
            's', "step",
            HelpText = "Area size step",
            Default = -0.2)]
        public double Step { get; set; }

        [Option(
            'c', "count",
            HelpText = "Series count",
            Default = 30)]
        public int Count { get; set; }

        [Option(
            'd', "directory",
            HelpText = "Results directory name",
            Default = "exps")]
        public string Dir { get; set; } = "exps";

        public override void Run()
        {
            var sizes = Enumerable
                .Range(0, Count)
                .Select(i => Origin + i * Step);

            var tickSteps = new[]
            {
                0.01, 0.02, 0.05, 0.075,
                0.1,  0.2,  0.5,  0.75,
                1,    2,    5,    7.5
            };

            if (Directory.Exists(Dir))
            {
                Directory.Delete(Dir, true);
            }

            Directory.CreateDirectory(Dir);
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
                    new Complex(-size/2, -size/2),
                    new Complex(size/2, size/2));

                var identity = Function.Identity;
                var func = identity.RightCompose($"exp(#)", Complex.Exp);
                using var plot = GetPlot();
                Draw(identity, area, plot.Canvas, plot.PreimageMask, tickStep);
                Draw(
                    func, area,
                    plot.Canvas, plot.ImageMask, tickStep,
                    Quality, Quality);

                plot.Canvas.Save(Path.Combine(Dir, $"exp{i}.png"));
            }
        }
    }
}