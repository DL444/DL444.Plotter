using System;

namespace DL444.Plotter.Library
{
    public abstract class CircleBase : IGraphic
    {
        public CircleBase(int hRes, int vRes)
        {
            ExceptionHelper.CheckResolution(hRes, vRes);
            int spanLength = hRes * vRes / 32;
            HorizontalResolution = hRes;
            VerticalResolution = vRes;
            _frame = new Memory<int>(new int[spanLength]);
            Id = Guid.NewGuid();
        }
        public CircleBase(int hRes, int vRes, int radius) : this(hRes, vRes)
        {
            Radius = radius;
        }
        public CircleBase(int hRes, int vRes, int radius, Point center) : this(hRes, vRes, radius)
        {
            Center = center;
        }

        public Point Center
        {
            get => _center;
            set
            {
                if (Center != value)
                {
                    _center = value;
                    Dirty = true;
                }
            }
        }
        public int Radius
        {
            get => _radius;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(Radius), "Radius cannot be less than 0.");
                }
                if (Radius != value)
                {
                    _radius = value;
                    Dirty = true;
                }
            }
        }

        public Guid Id { get; }
        public bool Dirty { get; protected set; }

        public int HorizontalResolution { get; }
        public int VerticalResolution { get; }
        public int PixelCount => HorizontalResolution * VerticalResolution;

        public ReadOnlySpan<int> Frame => _frame.Span;

        public void ClearFrame()
        {
            WritableSpan.ClearAll();
        }
        public void Draw()
        {
            if (Dirty)
            {
                ClearFrame();
                if (Radius > 0)
                {
                    DrawEx();
                }
                else
                {
                    WritableSpan.SetBit(this.GetSpanIndex(Center.X, Center.Y), true);
                }
                Dirty = false;
            }
        }
        public override string ToString() => $"C = {Center}, R = {Radius}";

        protected Span<int> WritableSpan => _frame.Span;

        protected abstract void DrawEx();
        protected (int, int) GetAbsoluteCoordinate(int relX, int relY) => (relX + Center.X, relY + Center.Y);
        protected void DrawRelativePoint(int relX, int relY)
        {
            int x;
            int y;
            (x, y) = GetAbsoluteCoordinate(relX, relY);
            WritableSpan.SetBit(this.GetSpanIndex(x, y), true);
            (x, y) = GetAbsoluteCoordinate(-relX, relY);
            WritableSpan.SetBit(this.GetSpanIndex(x, y), true);
            (x, y) = GetAbsoluteCoordinate(relX, -relY);
            WritableSpan.SetBit(this.GetSpanIndex(x, y), true);
            (x, y) = GetAbsoluteCoordinate(-relX, -relY);
            WritableSpan.SetBit(this.GetSpanIndex(x, y), true);
            (x, y) = GetAbsoluteCoordinate(relY, relX);
            WritableSpan.SetBit(this.GetSpanIndex(x, y), true);
            (x, y) = GetAbsoluteCoordinate(-relY, relX);
            WritableSpan.SetBit(this.GetSpanIndex(x, y), true);
            (x, y) = GetAbsoluteCoordinate(relY, -relX);
            WritableSpan.SetBit(this.GetSpanIndex(x, y), true);
            (x, y) = GetAbsoluteCoordinate(-relY, -relX);
            WritableSpan.SetBit(this.GetSpanIndex(x, y), true);
        }

        private readonly Memory<int> _frame;
        private Point _center;
        private int _radius;
    }

    public sealed class MidpointCircle : CircleBase
    {
        public MidpointCircle(int hRes, int vRes) : base(hRes, vRes) { }
        public MidpointCircle(int hRes, int vRes, int radius) : base(hRes, vRes, radius) { }
        public MidpointCircle(int hRes, int vRes, int radius, Point center) : base(hRes, vRes, radius, center) { }

        protected override void DrawEx()
        {
            int x = 0;
            int y = Radius;
            DrawRelativePoint(x, y);
            double discriminant = 1.25 - Radius;
            while (y >= x)
            {
                x++;
                if (discriminant < 0.0)
                {
                    DrawRelativePoint(x, y);
                    discriminant += 2.0 * x + 3.0;
                }
                else
                {
                    DrawRelativePoint(x, y - 1);
                    discriminant += 2.0 * (x - y) + 5.0;
                    y--;
                }
            }
        }
    }

    public sealed class BresenhamCircle : CircleBase
    {
        public BresenhamCircle(int hRes, int vRes) : base(hRes, vRes) { }
        public BresenhamCircle(int hRes, int vRes, int radius) : base(hRes, vRes, radius) { }
        public BresenhamCircle(int hRes, int vRes, int radius, Point center) : base(hRes, vRes, radius, center) { }

        protected override void DrawEx()
        {
            int x = 0;
            int y = Radius;
            DrawRelativePoint(x, y);
            int disD = 2 - 2 * Radius;
            while (y >= x)
            {
                PointSelection selection;
                if (disD == 0)
                {
                    selection = PointSelection.DownRight;
                }
                else if (disD > 0)
                {
                    int dIntersect = 2 * (disD - x) - 1;
                    selection = dIntersect < 0 ? PointSelection.DownRight: PointSelection.Down;
                }
                else
                {
                    int dIntersect = 2 * (disD + y) - 1;
                    selection = dIntersect < 0 ? PointSelection.Right : PointSelection.DownRight;
                }

                if (selection == PointSelection.DownRight)
                {
                    x++;
                    y--;
                    disD += 2 * (x - y) + 2;
                    DrawRelativePoint(x, y);
                }
                else if (selection == PointSelection.Down)
                {
                    y--;
                    disD += 1 - 2 * y;
                    DrawRelativePoint(x, y);
                }
                else
                {
                    x++;
                    disD += 2 * x + 1;
                    DrawRelativePoint(x, y);
                }
            }
        }

        private enum PointSelection
        {
            Down, Right, DownRight
        }
    }
}
