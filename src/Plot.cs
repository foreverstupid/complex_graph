using System;
using System.Drawing;

namespace ComplexGraph
{
    /// <summary>
    /// Help class containig info about creating picture.
    /// </summary>
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