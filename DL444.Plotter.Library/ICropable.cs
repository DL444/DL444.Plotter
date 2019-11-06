using System;

namespace DL444.Plotter.Library
{
    public interface ICropable : IGraphic
    {
        CropWindow? CropWindow { get; set; }
    }

    public struct CropWindow
    {
        public CropWindow(Point p0, Point p1)
        {
            Point0 = p0;
            Point1 = p1;
        }

        public Point Point0 { get; set; }
        public Point Point1 { get; set; }

        public int MinX => Math.Min(Point0.X, Point1.X);
        public int MaxX => Math.Max(Point0.X, Point1.X);
        public int MinY => Math.Min(Point0.Y, Point1.Y);
        public int MaxY => Math.Max(Point0.Y, Point1.Y);

        public override bool Equals(object obj)
        {
            if (obj is CropWindow win)
            {
                return this.Point0 == win.Point0 && this.Point1 == win.Point1;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(CropWindow lhs, CropWindow rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(CropWindow lhs, CropWindow rhs) => !(lhs == rhs);
    }

    internal static class CropHelper
    {
        public static AreaCode GetAreaCode(CropWindow window, Point pt)
        {
            AreaCode code = AreaCode.Center;

            if (pt.X < window.MinX)
            {
                code = AreaCode.Left;
            }
            else if (pt.X > window.MaxX)
            {
                code = AreaCode.Right;
            }

            if (pt.Y < window.MinY)
            {
                code |= AreaCode.Down;
            }
            else if (pt.Y > window.MaxY)
            {
                code |= AreaCode.Up;
            }

            return code;
        }
    }

    [Flags]
    internal enum AreaCode
    {
        Center = 0, Left = 1, Right = 2, Down = 4, Up = 8
    }

}
