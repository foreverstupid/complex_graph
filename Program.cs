using System.Reflection.Emit;
using System;
using System.Drawing;
using System.Numerics;

namespace ComplexGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            var area = new Area(
                new Complex(-10, -10),
                new Complex(10, 10));

            using var bitmap = area.ToBitmap(1000, 1000);
            bitmap.Save("plot.png");
        }
    }
}
