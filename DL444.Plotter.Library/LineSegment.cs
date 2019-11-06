using System;
using System.Collections.Generic;

namespace DL444.Plotter.Library
{
    public abstract class LineSegmentBase : IGraphic, IFrame, ICropable
    {
        public LineSegmentBase(int hRes, int vRes)
        {
            ExceptionHelper.CheckResolution(hRes, vRes);
            int spanLength = hRes * vRes / 32;
            HorizontalResolution = hRes;
            VerticalResolution = vRes;
            _frame = new Memory<int>(new int[spanLength]);
            Id = Guid.NewGuid();
        }
        public LineSegmentBase(int hRes, int vRes, Point p0, Point p1) : this(hRes, vRes)
        {
            Point0 = p0;
            Point1 = p1;
        }

        public Point Point0
        {
            get => _point0;
            set
            {
                if (Point0 != value)
                {
                    _point0 = value;
                    Dirty = true;
                }
            }
        }
        public Point Point1
        {
            get => _point1;
            set
            {
                if (Point1 != value)
                {
                    _point1 = value;
                    Dirty = true;
                }
            }
        }
        public double Slope => (Point0.Y - Point1.Y) / ((double)Point0.X - Point1.X);

        public bool Dirty { get; protected set; } = true;

        public ReadOnlySpan<int> Frame => _frame.Span;
        public Guid Id { get; }

        public int HorizontalResolution { get; }
        public int VerticalResolution { get; }
        public int PixelCount => HorizontalResolution * VerticalResolution;

        public CropWindow? CropWindow
        {
            get => _cropWindow;
            set
            {
                if (CropWindow != value)
                {
                    _cropWindow = value;
                    Dirty = true;
                }
            }
        }

        public void Draw()
        {
            if (Dirty)
            {
                ClearFrame();
                var state = CropPredetermine();
                if (state == CropState.All)
                {
                    return;
                }

                if (!DrawHorizontalOrVerticalLineEx(state))
                {
                    int startX, endX, startY, endY;
                    bool flip;
                    double k;
                    if (state == CropState.None)
                    {
                        (startX, endX, startY, endY, flip, k) = GetDrawInitialMetrics(Point0, Point1);
                    }
                    else
                    {
                        (var p0, var p1) = GetIntersects();
                        (startX, endX, startY, endY, flip, k) = GetDrawInitialMetrics(p0, p1);
                    }
                    DrawEx(startX, endX, startY, endY, flip, k);
                }
                Dirty = false;
            }
        }
        public void ClearFrame()
        {
            WritableSpan.ClearAll();
        }
        public override string ToString() => $"P0 = {Point0}, P1 = {Point1}";

        protected Span<int> WritableSpan => _frame.Span;

        protected abstract void DrawEx(int startX, int endX, int startY, int endY, bool flip, double k);
        protected (int, int, int, int, bool, double) GetDrawInitialMetrics(Point p0, Point p1)
        {
            bool flip = Slope > 1.0 || Slope < -1.0;

            int startX;
            int endX;
            int startY;
            int endY;
            double k;
            if (!flip)
            {
                if (p0.X < p1.X)
                {
                    startX = p0.X;
                    endX = p1.X;
                    startY = p0.Y;
                    endY = p1.Y;
                }
                else
                {
                    startX = p1.X;
                    endX = p0.X;
                    startY = p1.Y;
                    endY = p0.Y;
                }
                k = Slope;
            }
            else
            {
                if (p0.Y < p1.Y)
                {
                    startX = p0.Y;
                    endX = p1.Y;
                    startY = p0.X;
                    endY = p1.X;
                }
                else
                {
                    startX = p1.Y;
                    endX = p0.Y;
                    startY = p1.X;
                    endY = p0.X;
                }
                k = 1.0 / Slope;
            }
            return (startX, endX, startY, endY, flip, k);
        }

        private readonly Memory<int> _frame;
        private Point _point0;
        private Point _point1;
        private CropWindow? _cropWindow;

        private CropState CropPredetermine()
        {
            if (CropWindow == null)
            {
                return CropState.None;
            }

            var code0 = CropHelper.GetAreaCode(CropWindow.Value, Point0);
            var code1 = CropHelper.GetAreaCode(CropWindow.Value, Point1);

            if (code0 == AreaCode.Center && code1 == AreaCode.Center)
            {
                return CropState.None;
            }
            else if ((code0 & code1) != 0)
            {
                return CropState.All;
            }
            else if (code0 == AreaCode.Center || code1 == AreaCode.Center)
            {
                return CropState.PartialInternal;
            }
            else
            {
                return CropState.PartialExternal;
            }
        }
        private bool DrawHorizontalOrVerticalLineEx(CropState cropState = CropState.None)
        {
            if (Point0 == Point1)
            {
                WritableSpan.SetBit(this.GetSpanIndex(Point0.X, Point0.Y), true);
                return true;
            }

            int min, max;
            if (double.IsInfinity(Slope))
            {
                min = Math.Min(Point0.Y, Point1.Y);
                max = Math.Max(Point0.Y, Point1.Y);

                if (cropState == CropState.PartialInternal || cropState == CropState.PartialExternal)
                {
                    min = Math.Max(min, CropWindow.Value.MinY);
                    max = Math.Min(max, CropWindow.Value.MaxY);
                }

                for (int i = min; i <= max; i++)
                {
                    WritableSpan.SetBit(this.GetSpanIndex(Point0.X, i), true);
                }
                return true;
            }
            else if (Slope == 0.0)
            {
                min = Math.Min(Point0.X, Point1.X);
                max = Math.Max(Point0.X, Point1.X);

                if (cropState == CropState.PartialInternal || cropState == CropState.PartialExternal)
                {
                    min = Math.Max(min, CropWindow.Value.MinX);
                    max = Math.Min(max, CropWindow.Value.MaxX);
                }

                for (int i = min; i <= max; i++)
                {
                    WritableSpan.SetBit(this.GetSpanIndex(i, Point0.Y), true);
                }
                return true;
            }

            return false;
        }
        private (Point, Point) GetIntersects()
        {
            List<Point> points = new List<Point>(6);
            points.Add(Point0);
            points.Add(Point1);
            points.Add(new Point(CropWindow.Value.MaxX, GetY(CropWindow.Value.MaxX)));
            points.Add(new Point(CropWindow.Value.MinX, GetY(CropWindow.Value.MinX)));
            points.Add(new Point(GetX(CropWindow.Value.MaxY), CropWindow.Value.MaxY));
            points.Add(new Point(GetX(CropWindow.Value.MinY), CropWindow.Value.MinY));
            points.Sort((p0, p1) => p0.X - p1.X);

            return (points[2], points[3]);

            int GetY(int x)
            {
                return (int)Math.Round(Point0.Y + Slope * (x - Point0.X));
            }
            int GetX(int y)
            {
                return (int)Math.Round(Point0.X + (y - Point0.Y) / Slope);
            }
        }
        private enum CropState
        {
            None, All, PartialInternal, PartialExternal
        }
    }

    public sealed class DdaLineSegment : LineSegmentBase
    {
        public DdaLineSegment(int hRes, int vRes) : base(hRes, vRes) { }
        public DdaLineSegment(int hRes, int vRes, Point p0, Point p1) : base(hRes, vRes, p0, p1) { }

        protected override void DrawEx(int startX, int endX, int startY, int endY, bool flip, double k)
        {
            int x = startX;
            double y = startY;

            for (; x <= endX; x++)
            {
                if (!flip)
                {
                    WritableSpan.SetBit(this.GetSpanIndex(x, (int)Math.Round(y)), true);
                }
                else
                {
                    WritableSpan.SetBit(this.GetSpanIndex((int)Math.Round(y), x), true);
                }
                y += k;
            }

        }
    }

    public sealed class MidpointLineSegment : LineSegmentBase
    {
        public MidpointLineSegment(int hRes, int vRes) : base(hRes, vRes) { }
        public MidpointLineSegment(int hRes, int vRes, Point p0, Point p1) : base(hRes, vRes, p0, p1) { }

        protected override void DrawEx(int x, int endX, int y, int endY, bool flip, double k)
        {
            int inc = Math.Sign(k);
            double d = -0.5;

            for (; x <= endX; x++)
            {
                int currY = y;
                if (d > 0)
                {
                    currY += inc;
                }

                if (!flip)
                {
                    WritableSpan.SetBit(this.GetSpanIndex(x, currY), true);
                }
                else
                {
                    WritableSpan.SetBit(this.GetSpanIndex(currY, x), true);
                }

                d += Math.Abs(k);

                if (d >= 0.5)
                {
                    d -= 1.0;
                    y += inc;
                }
            }
        }
    }

    public sealed class BresenhamLineSegment : LineSegmentBase
    {
        public BresenhamLineSegment(int hRes, int vRes) : base(hRes, vRes) { }
        public BresenhamLineSegment(int hRes, int vRes, Point p0, Point p1) : base(hRes, vRes, p0, p1) { }

        protected override void DrawEx(int x, int endX, int y, int endY, bool flip, double k)
        {
            int dX = endX - x;
            int dY = endY - y;
            int inc = Math.Sign(k);
            int d = -dX;

            for (; x <= endX; x++)
            {
                int currY = y;
                if (d > 0)
                {
                    currY += inc;
                }

                if (!flip)
                {
                    WritableSpan.SetBit(this.GetSpanIndex(x, currY), true);
                }
                else
                {
                    WritableSpan.SetBit(this.GetSpanIndex(currY, x), true);
                }

                d += Math.Abs(2 * dY);

                if (d >= dX)
                {
                    d -= 2 * dX;
                    y += inc;
                }
            }
        }
    }
}
