using System;
using System.Drawing;
using System.Drawing.Imaging;
using CommandLine;

namespace ComplexGraph.Verbs
{
    /// <summary>
    /// Base class for all verbs.
    /// </summary>
    abstract class Verb
    {
        [Option(
            'w', "width",
            HelpText = "The width of the preimage in the resulting plot picture",
            Default = 1000)]
        public int Width { get; set; }

        [Option(
            'h', "height",
            HelpText = "The height of the preimage in the resulting plot picture",
            Default = 1000)]
        public int Height { get; set; }

        [Option(
            'q', "quality",
            HelpText = "Quality of drawing in count of dots along an axis in preimage",
            Default = 8000)]
        public int Quality { get; set; }

        /// <summary>
        /// Performs the verb actions.
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// Draws the given function into the bitmap according to the
        /// given mask on it.
        /// </summary>
        protected static void Draw(
            Function func,
            Area area,
            Bitmap plot,
            Rectangle mask,
            double tickStep,
            int? xCount = null,
            int? yCount = null)
        {
            var scan = plot.LockBits(mask, ImageLockMode.ReadWrite, Image.Format);
            var holder = new Image(scan);

            func.DrawTo(area, holder, xCount, yCount);
            holder.CopyToBitmapScan(scan);
            plot.UnlockBits(scan);

            plot.DrawCoordinateAxes(mask, area, tickStep, tickStep);
            plot.DrawFuncName(mask, func.Name.Value);
        }

        /// <summary>
        /// Creates and returns all info about creating plot picture.
        /// </summary>
        protected Plot GetPlot()
        {
            int margin = 10;
            int spaceBetween = 100;
            var background = new SolidBrush(Color.LightGray);

            var bitmap = new Bitmap(
                4 * margin + spaceBetween + 2 * Width,
                2 * margin + Height);

            var preimageMask = new Rectangle(margin, margin, Width, Height);
            var imageMask = new Rectangle(
                3 * margin + spaceBetween + Width,
                margin,
                Width,
                Height);

            using var graph = Graphics.FromImage(bitmap);
            graph.FillRectangle(background, 0, 0, bitmap.Width, bitmap.Height);

            return new Plot(bitmap, preimageMask, imageMask);
        }
    }
}