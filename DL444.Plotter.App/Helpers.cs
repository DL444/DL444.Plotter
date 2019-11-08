using DL444.Plotter.Library;

namespace DL444.Plotter.App
{
    internal static class CoordinateHelper
    {
        public static Point GetMirroredPoint(Point point, int vRes)
        {
            return GetMirroredPoint(point.X, point.Y, vRes);
        }
        public static Point GetMirroredPoint(int x, int y, int vRes)
        {
            return new Point(x, GetMirroredY(y, vRes));
        }
        public static int GetMirroredY(int y, int vRes)
        {
            return vRes - 1 - y;
        }
    }
}
