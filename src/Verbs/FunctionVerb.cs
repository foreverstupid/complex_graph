using System;
using System.Numerics;
using CommandLine;

namespace ComplexGraph.Verbs
{
    [Verb("func", isDefault: true, HelpText = "Draws a complex-valued function")]
    class FunctionVerb : Verb
    {
        private const string Reference = "The description of the drawing function.\n" +
            "You can use the following operations: +, -, *, /, ^, (), " +
            "exp, ln, sin, cos, tan.\n" +
            "Term 'z' is used for marking an argument.\n" +
            "Complex constant can be written as <real> or " +
            "<imaginary>i or {<real>,<imaginary>i}. E.g.: 1, 2i, {3,0.5i}.\n" +
            "Examples of function descriptions: 2 * (sin exp z^3 / tan ln z^z), " +
            "(sin z)^(2i) + cos(z * {3,0.1i} + z*z)";

        [Option('d', "description", HelpText = Reference, Required = true)]
        public string FuncDescription { get; set; } = "";

        [Option(
            'l', "left",
            HelpText = "Coordinate of the left side of preimage area (min Re z)",
            Default = -1.0)]
        public double Left { get; set; }

        [Option(
            'r', "right",
            HelpText = "Coordinate of the right side of preimage area (max Re z)",
            Default = 1.0)]
        public double Right { get; set; }

        [Option(
            'b', "bottom",
            HelpText = "Coordinate of the bottom side of preimage area (min Im z)",
            Default = -1.0)]
        public double Bottom { get; set; }

        [Option(
            't', "top",
            HelpText = "Coordinate of the top side of preimage area (max Im z)",
            Default = 1.0)]
        public double Top { get; set; }

        [Option(
            'f', "file-name",
            HelpText = "The name of the creating plot file",
            Default = "plot.png")]
        public string FileName { get; set; } = "plot.png";

        public override void Run()
        {
            using var plot = GetPlot();
            double tickStep = GetTicksStep();
            var area = new Area(
                new Complex(Left, Bottom),
                new Complex(Right, Top));

            var identity = Function.Identity;
            var func = FuncDescription.Parse();

            Draw(identity, area, plot.Canvas, plot.PreimageMask, tickStep);
            Draw(func, area, plot.Canvas, plot.ImageMask, tickStep, Quality, Quality);
            plot.Canvas.Save(FileName);
        }

        private double GetTicksStep()
        {
            return 1.0;
        }
    }
}