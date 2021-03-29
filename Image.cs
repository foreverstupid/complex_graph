using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace ComplexGraph
{
    /// <summary>
    /// Byte array as image representation.
    /// Each pixel is coded by three bytes: R, G, and B.
    /// </summary>
    public class Image
    {
        public const int Depth = 3;
        public const PixelFormat Format = PixelFormat.Format24bppRgb;

        private readonly byte[] data;

        public Image(int width, int height)
        {
            Width = width;
            Height = height;
            this.data = new byte[width * height * Depth];
        }

        public Image(BitmapData scan)
        {
            Width = scan.Width;
            Height = scan.Height;
            this.data = new byte[Width * Height * Depth];
            Marshal.Copy(scan.Scan0, this.data, 0, this.data.Length);
        }

        public int Width { get; }

        public int Height { get; }

        public void SetPixel(int xPos, int yPos, Color c)
        {
            int offset = (yPos * Width + xPos) * Depth;
            data[offset] = c.B;
            data[offset + 1] = c.G;
            data[offset + 2] = c.R;
        }

        public void CopyToBitmapScan(BitmapData scan)
            => Marshal.Copy(data, 0, scan.Scan0, this.data.Length);
    }
}