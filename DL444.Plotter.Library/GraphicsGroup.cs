using System;
using System.Collections;
using System.Collections.Generic;

namespace DL444.Plotter.Library
{
    public sealed class GraphicsGroup : IGraphic, IList<IGraphic>
    {
        public GraphicsGroup(int hRes, int vRes)
        {
            ExceptionHelper.CheckResolution(hRes, vRes);
            int spanLength = hRes * vRes / 32;
            HorizontalResolution = hRes;
            VerticalResolution = vRes;
            _frame = new Memory<int>(new int[spanLength]);
            Id = Guid.NewGuid();
        }
        public GraphicsGroup(int hRes, int vRes, IEnumerable<IGraphic> graphics) : this(hRes, vRes)
        {
            this.graphics = new List<IGraphic>(graphics);
        }

        public Guid Id { get; }
        public bool Dirty => true;

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
            composer.Compose(graphics).CopyTo(_frame.Span);
        }

        #region IList Implementation
        public int Count => graphics.Count;

        public bool IsReadOnly => ((IList<IGraphic>)graphics).IsReadOnly;

        public IGraphic this[int index] { get => graphics[index]; set => graphics[index] = value; }

        public int IndexOf(IGraphic item)
        {
            return graphics.IndexOf(item);
        }

        public void Insert(int index, IGraphic item)
        {
            graphics.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            graphics.RemoveAt(index);
        }

        public void Add(IGraphic item)
        {
            graphics.Add(item);
        }

        public void Clear()
        {
            graphics.Clear();
        }

        public bool Contains(IGraphic item)
        {
            return graphics.Contains(item);
        }

        public void CopyTo(IGraphic[] array, int arrayIndex)
        {
            graphics.CopyTo(array, arrayIndex);
        }

        public bool Remove(IGraphic item)
        {
            return graphics.Remove(item);
        }

        public IEnumerator<IGraphic> GetEnumerator()
        {
            return ((IList<IGraphic>)graphics).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<IGraphic>)graphics).GetEnumerator();
        }
        #endregion

        private readonly List<IGraphic> graphics = new List<IGraphic>();
        private readonly ComposerExecutive composer;
        private readonly Memory<int> _frame;
    }
}
