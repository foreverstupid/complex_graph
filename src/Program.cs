using CommandLine;
using ComplexGraph.Verbs;

namespace ComplexGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<FunctionVerb, PowsVerb, ExpsVerb, ExpamplesVerb>(args)
                .WithParsed<FunctionVerb>(v => v.Run())
                .WithParsed<PowsVerb>(v => v.Run())
                .WithParsed<ExpsVerb>(v => v.Run())
                .WithParsed<ExpamplesVerb>(v => v.Run());
        }
    }
}
