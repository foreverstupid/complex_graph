using System;
using System.Drawing;

namespace ComplexGraph
{
    public class Plot : IDisposable
    {
        public Plot(
            Bitmap canvas,
            Rectangle preimageMask,
            Rectangle imageMask)
        {
            Canvas = canvas;
            PreimageMask = preimageMask;
            ImageMask = imageMask;
        }

        public Bitmap Canvas { get; }

        public Rectangle PreimageMask { get; }

        public Rectangle ImageMask { get; }

        public void Dispose()
            => Canvas.Dispose();
    }
}