using System;

namespace DL444.Plotter.Library
{
    public abstract class EllipseBase : IGraphic
    {
        public EllipseBase(int hRes, int vRes)
        {
            ExceptionHelper.CheckResolution(hRes, vRes);
            int spanLength = hRes * vRes / 32;
            HorizontalResolution = hRes;
            VerticalResolution = vRes;
            _frame = new Memory<int>(new int[spanLength]);
            Id = Guid.NewGuid();
        }
        public EllipseBase(int hRes, int vRes, int a, int b) : this(hRes, vRes)
        {
            A = a;
            B = b;
        }
        public EllipseBase(int hRes, int vRes, int a, int b, Point center) : this(hRes, vRes, a, b)
        {
            Center = center;
        }


        public int A
        {
            get => _a;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(A), "A cannot be less than 0.");
                }
                if (A != value)
                {
                    _a = value;
                    Dirty = true;
                }
            }
        }
        public int B
        {
            get => _b;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(B), "B cannot be less than 0.");
                }
                if (B != value)
                {
                    _b = value;
                    Dirty = true;
                }
            }
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

        public Guid Id { get; }
        public bool Dirty { get; protected set; } = true;

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
                if (B == 0)
                {

                    WritableSpan.SetBits(this.GetSpanIndex(Center.X - A, Center.Y), this.GetSpanIndex(Center.X + A + 1, Center.Y), true);
                }
                else
                {
                    DrawEx();
                }
                Dirty = false;
            }
        }
        public override string ToString() => $"C = {Center}, A = {A}, B = {B}";

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
        }

        private readonly Memory<int> _frame;
        private int _a;
        private int _b;
        private Point _center;
    }

    public sealed class MidpointEllipse : EllipseBase
    {
        public MidpointEllipse(int hRes, int vRes) : base(hRes, vRes) { }
        public MidpointEllipse(int hRes, int vRes, int a, int b) : base(hRes, vRes, a, b) { }
        public MidpointEllipse(int hRes, int vRes, int a, int b, Point center) : base(hRes, vRes, a, b, center) { }

        protected override void DrawEx()
        {
            int x = 0;
            int y = B;
            DrawRelativePoint(x, y);
            double discriminant = B * B + A * A * (0.25 - B);
            while (B * B * (x + 1) < A * A * (y - 0.5))
            {
                if (discriminant < 0)
                {
                    discriminant += B * B * (2 * x + 3);
                    x++;
                    DrawRelativePoint(x, y);
                }
                else
                {
                    discriminant += B * B * (2 * x + 3) + A * A * (2 - 2 * y);
                    x++;
                    y--;
                    DrawRelativePoint(x, y);
                }
            }

            discriminant = B * B * (x + 0.5) * (x + 0.5)
                + A * A * (y - 1) * (y - 1)
                - A * A * B * B;
            while (y >= 0)
            {
                if (discriminant > 0)
                {
                    discriminant += A * A * (3 - 2 * y);
                    y--;
                    DrawRelativePoint(x, y);
                }
                else
                {
                    discriminant += A * A * (3 - 2 * y) + B * B * (2 * x + 2);
                    x++;
                    y--;
                    DrawRelativePoint(x, y);
                }
            }
        }
    }
}
