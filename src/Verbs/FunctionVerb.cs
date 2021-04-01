using System;
using System.Numerics;
using CommandLine;

namespace ComplexGraph.Verbs
{
    [Verb("func", isDefault: true, HelpText = "Draws a complex-valued function")]
    class FunctionVerb : Verb
    {
        private const string Reference = "The description of the drawing function.\n" +
            "You can use the following operations: +, -, *, /, ^, " +
            "exp, ln, sin, cos, tan. " +
            "Term 'z' is used for marking an argument.\n\n" +
            "Complex constants can be written as <real> or " +
            "<imaginary>i or {<real>,<imaginary>i}. E.g.: 1, 2i, {3,0.5i}.\n\n" +
            "Unary operations have more priority. For example ln z^2 actually " +
            "means (ln z)^2. To change priority use parantheses, e.g. ln (z^2)\n\n" +
            "Note, if description starts with a minus (e.g. '-ln z'), then it should be " +
            "the last argument and be placed after '--'. E.g. -- '-ln z'. It is " +
            "due to CMD arguments parsing mechanism.\n\n" +
            "Examples of function descriptions:\n" +
            "    2 * (sin exp z^3 / tan ln z^z)\n" +
            "    (sin z)^2i + cos(z * {3,0.1i} + z*z)";

        [Value(0, HelpText = Reference, Required = true)]
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
            "tick-step",
            HelpText = "Tick step along axes. If not specified, " +
                       "then it is chosen automatically")]
        public double? TicksStep { get; set; }

        [Option(
            'f', "file-name",
            HelpText = "The name of the creating plot file",
            Default = "plot.png")]
        public string FileName { get; set; } = "plot.png";

        public override void Run()
        {
            using var plot = GetPlot();
            var area = new Area(
                new Complex(Left, Bottom),
                new Complex(Right, Top));

            double tickStep = TicksStep ?? GetTicksStep(area);
            var identity = Function.Identity;
            var func = FuncDescription.Parse();

            Draw(identity, area, plot.Canvas, plot.PreimageMask, tickStep);
            Draw(func, area, plot.Canvas, plot.ImageMask, tickStep, Quality, Quality);
            plot.Canvas.Save(FileName);
        }

        private static double GetTicksStep(Area area)
        {
            var size = Math.Max(area.Width, area.Height);
            var lg = Math.Round(Math.Log10(size));
            var step = Math.Pow(10, lg);
            while (step > size / 5)
            {
                step /= 5;
            }

            return step;
        }
    }
}