using System;
using System.Drawing;

namespace ComplexGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            const int size = 1000;

            using var bitmap = new Bitmap(size, size);
            using var graph = Graphics.FromImage(bitmap);

            var brush = new SolidBrush(Color.Green);
            var pen = new Pen(brush);

            for (int radius = 10; radius < 400; radius += 10)
            {
                var center = size / 2;
                graph.DrawEllipse(
                    pen,
                    new Rectangle(
                        center - radius,
                        center - radius,
                        2 * radius,
                        2 * radius));
            }

            bitmap.Save("plot.png");
        }
    }
}
