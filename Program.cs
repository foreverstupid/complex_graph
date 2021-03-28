using System;
using System.Drawing;
using System.Numerics;

namespace ComplexGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            var preimage = new Area(
                new Complex(-2, -2),
                new Complex(2, 2));

            var image = preimage.Apply(c => Complex.Sqrt(c), "sqrt");

            using var plot = GetPlot();
            preimage.DrawTo(plot.Canvas, plot.PreimageMask);
            image.DrawTo(plot.Canvas, plot.ImageMask, 4000, 4000);

            plot.Canvas.Save("plot.png");
        }

        private static Plot GetPlot()
        {
            int margin = 10;
            int spaceBetween = 100;
            int areaSize = 500;
            var background = new SolidBrush(Color.LightGray);

            var bitmap = new Bitmap(
                4 * margin + spaceBetween + 2 * areaSize,
                2 * margin + areaSize);

            var preimageMask = new Rectangle(margin, margin, areaSize, areaSize);
            var imageMask = new Rectangle(
                3 * margin + spaceBetween + areaSize,
                margin,
                areaSize,
                areaSize);

            using var graph = Graphics.FromImage(bitmap);
            graph.FillRectangle(background, 0, 0, bitmap.Width, bitmap.Height);

            return new Plot()
            {
                Canvas = bitmap,
                PreimageMask = preimageMask,
                ImageMask = imageMask,
            };
        }

        private class Plot : IDisposable
        {
            public Bitmap Canvas { get; init; }

            public Rectangle PreimageMask { get; init; }

            public Rectangle ImageMask { get; init; }

            public void Dispose()
                => Canvas.Dispose();
        }
    }
}
