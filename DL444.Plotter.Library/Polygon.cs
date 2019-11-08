using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DL444.Plotter.Library
{
    public abstract class PolygonBase : IGraphic, IList<Point>
    {
        public PolygonBase(int hRes, int vRes)
        {
            ExceptionHelper.CheckResolution(hRes, vRes);
            int spanLength = hRes * vRes / 32;
            HorizontalResolution = hRes;
            VerticalResolution = vRes;
            _frame = new Memory<int>(new int[spanLength]);
            Id = Guid.NewGuid();
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
                if (Count == 1)
                {
                    WritableSpan.SetBit(this.GetSpanIndex(this[0].X, this[0].Y), true);
                }
                else if (Count > 1)
                {
                    DrawEx();
                }
                Dirty = false;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("Plg: ");
            foreach (var p in this)
            {
                builder.Append($"{p} ");
            }
            return builder.ToString();
        }

        protected Span<int> WritableSpan => _frame.Span;

        protected abstract void DrawEx();

        #region IList Implementation
        public int Count => ((IList<Point>)points).Count;

        public bool IsReadOnly => ((IList<Point>)points).IsReadOnly;

        public Point this[int index] { get => ((IList<Point>)points)[index]; set => ((IList<Point>)points)[index] = value; }

        public int IndexOf(Point item)
        {
            return ((IList<Point>)points).IndexOf(item);
        }

        public void Insert(int index, Point item)
        {
            ((IList<Point>)points).Insert(index, item);
            Dirty = true;
        }

        public void RemoveAt(int index)
        {
            ((IList<Point>)points).RemoveAt(index);
            Dirty = true;
        }

        public void Add(Point item)
        {
            ((IList<Point>)points).Add(item);
            Dirty = true;
        }

        public void Clear()
        {
            ((IList<Point>)points).Clear();
            Dirty = true;
        }

        public bool Contains(Point item)
        {
            return ((IList<Point>)points).Contains(item);
        }

        public void CopyTo(Point[] array, int arrayIndex)
        {
            ((IList<Point>)points).CopyTo(array, arrayIndex);
        }

        public bool Remove(Point item)
        {
            var result = ((IList<Point>)points).Remove(item);
            if (result == true)
            {
                Dirty = true;
            }
            return result;
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return ((IList<Point>)points).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Point>)points).GetEnumerator();
        }
        #endregion

        private Memory<int> _frame;
        private List<Point> points = new List<Point>();
    }

    public sealed class ScanlinePolygon : PolygonBase
    {
        public ScanlinePolygon(int hRes, int vRes) : base(hRes, vRes) { }

        protected override void DrawEx()
        {
            var segments = GetSegments();
            var buckets = GetBuckets();
            List<BucketNode> nodes = new List<BucketNode>();
            (int minY, int maxY) = this.MinMax(x => x.Y);

            for (int y = minY; y <= maxY;)
            {
                if (buckets[y - minY].Count != 0)
                {
                    nodes.AddRange(buckets[y - minY]);
                    nodes.Sort();
                }
                for (int i = 0; i < nodes.Count; i += 2)
                {
                    int x0 = (int)Math.Round(nodes[i].X);
                    int x1 = (int)Math.Round(nodes[i + 1].X);
                    WritableSpan.SetBits(this.GetSpanIndex(x0, y), this.GetSpanIndex(x1, y) + 1, true);
                }
                y++;
                nodes.RemoveAll(x => x.MaxY == y);
                for (int i = 0; i < nodes.Count; i++)
                {
                    var n = nodes[i];
                    nodes[i] = new BucketNode(n.X + n.InvertSlope, n.InvertSlope, n.MaxY);
                }
            }
        }

        private Segment[] GetSegments()
        {
            var segments = new Segment[Count];
            for (int i = 0; i < Count - 1; i++)
            {
                segments[i] = new Segment(this[i], this[i + 1]);
            }
            segments[Count - 1] = new Segment(this[Count - 1], this[0]);
            return segments;
        }
        private List<BucketNode>[] GetBuckets()
        {
            var segments = GetSegments();
            (int minY, int maxY) = this.MinMax(x => x.Y);
            var buckets = new List<BucketNode>[maxY - minY + 1];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new List<BucketNode>();
            }
            foreach (var s in segments)
            {
                if (s.Slope == 0.0)
                {
                    continue;
                }

                int segMinY, segMinX, segMaxY;
                if (s.P0.Y > s.P1.Y)
                {
                    segMinX = s.P1.X;
                    segMinY = s.P1.Y;
                    segMaxY = s.P0.Y;
                }
                else
                {
                    segMinX = s.P0.X;
                    segMinY = s.P0.Y;
                    segMaxY = s.P1.Y;
                }
                buckets[segMinY - minY].Add(new BucketNode(segMinX, 1.0 / s.Slope, segMaxY));
            }
            foreach (var b in buckets)
            {
                b.Sort();
            }
            return buckets;
        }

        private struct Segment
        {
            public Segment(Point p0, Point p1)
            {
                P0 = p0;
                P1 = p1;
            }

            public Point P0 { get; set; }
            public Point P1 { get; set; }
            public double Slope => (P0.Y - P1.Y) / ((double)P0.X - P1.X);
        }
        private struct BucketNode : IComparable
        {
            public BucketNode(double x, double invertSlope, int maxY)
            {
                X = x;
                InvertSlope = invertSlope;
                MaxY = maxY;
            }

            public double X { get; set; }
            public double InvertSlope { get; set; }
            public int MaxY { get; set; }

            public int CompareTo(object obj)
            {
                if (obj is BucketNode y)
                {
                    if (this.X == y.X)
                    {
                        return this.InvertSlope.CompareTo(y.InvertSlope);
                    }
                    return this.X.CompareTo(y.X);
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}
