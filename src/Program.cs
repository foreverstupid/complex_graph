using System.Globalization;
using System;
using CommandLine;
using ComplexGraph.Verbs;

namespace ComplexGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var parser = new Parser(
                    c =>
                    {
                        c.HelpWriter = Console.Out;
                        c.EnableDashDash = true;
                        c.MaximumDisplayWidth = 80;
                        c.ParsingCulture = CultureInfo.InvariantCulture;
                    });

                parser
                    .ParseArguments<FunctionVerb, PowsVerb, ExpsVerb, ExpamplesVerb>(args)
                    .WithParsed<FunctionVerb>(v => v.Run())
                    .WithParsed<PowsVerb>(v => v.Run())
                    .WithParsed<ExpsVerb>(v => v.Run())
                    .WithParsed<ExpamplesVerb>(v => v.Run());
            }
            catch (Exception e)
            {
                HandleUncaughtError(e);
            }
        }

        private static void HandleUncaughtError(Exception e)
        {
            var c = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            catch
            {
                // some terminals can throw on changing color
            }

            Console.WriteLine(e.Message);

            try
            {
                Console.ForegroundColor = c;
            }
            catch
            {
                // some terminals can throw on changing color
            }
        }
    }
}
