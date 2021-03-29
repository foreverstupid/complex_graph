using System;
using System.Drawing;

namespace ComplexGraph
{
    /// <summary>
    /// Extensions methods for <see cref="Graphics"/>.
    /// </summary>
    internal static class GrpahicsExtensions
    {
        public static void DrawArrow(
            this Graphics g,
            Pen pen,
            float x1, float y1,
            float x2, float y2,
            float arrowSize)
        {
            g.DrawLine(pen, x1, y1, x2, y2);

            float dx = x2 - x1;
            float dy = y2 - y1;
            float norm = MathF.Sqrt(dx * dx + dy * dy);
            g.DrawArrowHead(
                pen,
                new PointF(x2, y2),
                dx / norm,
                dy / norm,
                arrowSize);
        }

        public static void DrawArrowHead(
            this Graphics g,
            Pen pen,
            PointF p,
            float dx, float dy,
            float size, float sweep = 0.125f * (float)Math.PI)
        {
            float ax1 = size * (MathF.Cos(sweep) * dx - MathF.Sin(sweep) * dy);
            float ay1 = size * (MathF.Sin(sweep) * dx + MathF.Cos(sweep) * dy);

            float ax2 = size * (MathF.Cos(sweep) * dx + MathF.Sin(sweep) * dy);
            float ay2 = size * (-MathF.Sin(sweep) * dx + MathF.Cos(sweep) * dy);

            var points = new[]
            {
                new PointF(p.X - ax1, p.Y - ay1),
                p,
                new PointF(p.X - ax2, p.Y - ay2),
            };

            g.DrawLines(pen, points);
        }
    }
}