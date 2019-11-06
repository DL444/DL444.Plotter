using System;
using System.Collections;
using System.Collections.Generic;

namespace DL444.Plotter.Library
{
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Point pt)
            {
                return this == pt;
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

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static bool operator ==(Point lhs, Point rhs)
        {
            return (lhs.X == rhs.X) && (lhs.Y == rhs.Y);
        }
        public static bool operator !=(Point lhs, Point rhs) => !(lhs == rhs);
    }

    public sealed class PointGraphic : IGraphic, IList<Point>
    {
        public PointGraphic(int hRes, int vRes)
        {
            ExceptionHelper.CheckResolution(hRes, vRes);
            int spanLength = hRes * vRes / 32;
            HorizontalResolution = hRes;
            VerticalResolution = vRes;
            _frame = new Memory<int>(new int[spanLength]);
            Id = Guid.NewGuid();
        }
        
        public Guid Id { get; }
        public bool Dirty { get; private set; } = true;

        public int HorizontalResolution { get; }
        public int VerticalResolution { get; }
        public int PixelCount => HorizontalResolution * VerticalResolution;

        public ReadOnlySpan<int> Frame => _frame.Span;

        public void ClearFrame()
        {
            _frame.Span.ClearAll();
        }

        public void Draw()
        {
            if (Dirty)
            {
                ClearFrame();
                foreach (var p in this)
                {
                    _frame.Span.SetBit(this.GetSpanIndex(p.X, p.Y), true);
                }
            }
        }

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

        private readonly Memory<int> _frame;
        private Point _point;
        private List<Point> points = new List<Point>();
    }
}
