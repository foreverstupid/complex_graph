using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

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
        private readonly double[] zorder;
        private readonly Func<Complex, double> getZorder;

        public Image(BitmapData scan, ZorderType zorderType = ZorderType.Magnitude)
        {
            Width = scan.Width;
            Height = scan.Height;

            this.data = new byte[Width * Height * Depth];
            this.zorder = new double[this.data.Length];
            this.getZorder = zorderType switch
            {
                ZorderType.Real => c => c.Real,
                ZorderType.Imaginary => c => c.Imaginary,
                ZorderType.Magnitude => c => c.Magnitude,
                _ => throw new ArgumentException("Unhandled z-order type"),
            };

            Array.Fill(zorder, double.NegativeInfinity);
            Marshal.Copy(scan.Scan0, this.data, 0, this.data.Length);
        }

        public enum ZorderType { Real, Magnitude, Imaginary }

        public int Width { get; }

        public int Height { get; }

        /// <summary>
        /// Sets the color for the given pixel.
        /// </summary>
        /// <param name="xPos">X-position of the pixel.</param>
        /// <param name="yPos">Y-position of the pixel.</param>
        /// <param name="c">Pixel color.</param>
        /// <param name="p">Complex point in preimage area that
        /// produced this pixel. It is used only for z-ordering
        /// (points of greater magnitude have more chances
        /// to be painted).</param>
        public void SetPixel(int xPos, int yPos, Color c, Complex p)
        {
            int offset = (yPos * Width + xPos) * Depth;
            double z = getZorder(p);
            if (InterlockedExchangeIfGreater(ref zorder[offset], z, z))
            {
                data[offset] = c.B;
                data[offset + 1] = c.G;
                data[offset + 2] = c.R;
            }
        }

        public void CopyToBitmapScan(BitmapData scan)
            => Marshal.Copy(data, 0, scan.Scan0, this.data.Length);

        /// <summary>
        /// Atomic operation that performs replacement <paramref name="location"/>
        /// with <paramref name="newValue"/> if <paramref name="location"/>
        /// is less than <paramref name="comparison"/>, returning true.
        /// Otherwise returns false.
        /// </summary>
        private static bool InterlockedExchangeIfGreater(
            ref double location,
            double comparison,
            double newValue)
        {
            double initialValue;
            do
            {
                initialValue = location;
                if (initialValue >= comparison)
                {
                    return false;
                }
            }
            while (Interlocked.CompareExchange(ref location, newValue, initialValue) != initialValue);
            return true;
        }
    }
}