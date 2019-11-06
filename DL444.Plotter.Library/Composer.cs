using System;
using System.Collections;
using System.Collections.Generic;

namespace DL444.Plotter.Library
{
    public sealed class Composer : IList<IGraphic>, IFrame
    {
        public Composer(int hRes, int vRes) : this(hRes, vRes, null) { }
        public Composer(int hRes, int vRes, Action<int, int, bool> drawAction)
        {
            ExceptionHelper.CheckResolution(hRes, vRes);
            HorizontalResolution = hRes;
            VerticalResolution = vRes;
            composer = new ComposerExecutive(hRes, vRes);
            DrawAction = drawAction;
        }

        public ReadOnlySpan<int> Frame => composer.Compose(graphics);
        public int HorizontalResolution { get; }
        public int VerticalResolution { get; }
        public int PixelCount => HorizontalResolution * VerticalResolution;
        public Action<int, int, bool> DrawAction { get; set; }

        public void TriggerDraw()
        {
            if (DrawAction == null)
            {
                return;
            }

            ReadOnlySpan<int> frame = Frame;
            for (int i = 0; i < PixelCount; i++)
            {
                DrawAction(i % HorizontalResolution, i / HorizontalResolution, frame.GetBit(i));
            }
        }

        #region IList implementation
        public IGraphic this[int index] { get => ((IList<IGraphic>)graphics)[index]; set => ((IList<IGraphic>)graphics)[index] = value; }

        public int Count => ((IList<IGraphic>)graphics).Count;

        public bool IsReadOnly => ((IList<IGraphic>)graphics).IsReadOnly;

        public void Add(IGraphic item)
        {
            ((IList<IGraphic>)graphics).Add(item);
        }

        public void Clear()
        {
            ((IList<IGraphic>)graphics).Clear();
        }

        public bool Contains(IGraphic item)
        {
            return ((IList<IGraphic>)graphics).Contains(item);
        }

        public void CopyTo(IGraphic[] array, int arrayIndex)
        {
            ((IList<IGraphic>)graphics).CopyTo(array, arrayIndex);
        }

        public IEnumerator<IGraphic> GetEnumerator()
        {
            return ((IList<IGraphic>)graphics).GetEnumerator();
        }

        public int IndexOf(IGraphic item)
        {
            return ((IList<IGraphic>)graphics).IndexOf(item);
        }

        public void Insert(int index, IGraphic item)
        {
            ((IList<IGraphic>)graphics).Insert(index, item);
        }

        public bool Remove(IGraphic item)
        {
            return ((IList<IGraphic>)graphics).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<IGraphic>)graphics).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<IGraphic>)graphics).GetEnumerator();
        }
        #endregion

        private readonly List<IGraphic> graphics = new List<IGraphic>();
        private readonly ComposerExecutive composer;
    }

    internal sealed class ComposerExecutive
    {
        public ComposerExecutive(int hRes, int vRes)
        {
            spanLength = hRes * vRes / 32;
        }

        private readonly int spanLength;
        public ReadOnlySpan<int> Compose(IEnumerable<IGraphic> graphics)
        {
            Span<int> span = new Span<int>(new int[spanLength]);
            foreach (var g in graphics)
            {
                if (g.Dirty)
                {
                    g.Draw();
                }

                for (int i = 0; i < span.Length; i++)
                {
                    span[i] |= g.Frame[i];
                }
            }
            return span;
        }
    }
}
