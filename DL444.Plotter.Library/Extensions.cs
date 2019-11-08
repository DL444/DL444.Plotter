using System;
using System.Collections.Generic;
using System.Linq;

namespace DL444.Plotter.Library
{
    public static class SpanExtensions
    {
        public static void SetBit(this Span<int> span, int index, bool value)
        {
            if (index < 0)
            {
                System.Diagnostics.Debug.WriteLine("SetBit(): Index less than 0.");
                return;
            }
            int intOffset = index / 32;
            //ExceptionHelper.CheckSpanLength(span, intOffset);
            int bitOffset = index % 32;
            int template = (int)(0x80000000 >> bitOffset);
            if (value == true)
            {
                span[intOffset] |= template;
            }
            else
            {
                span[intOffset] &= (~template);
            }
        }
        public static bool GetBit(this ReadOnlySpan<int> span, int index)
        {
            if (index < 0)
            {
                System.Diagnostics.Debug.WriteLine("GetBit(): Index less than 0.");
                return false;
            }
            int intOffset = index / 32;
            //ExceptionHelper.CheckSpanLength(span, intOffset);
            int bitOffset = index % 32;
            int template = (int)(0x80000000 >> bitOffset);
            int result = span[intOffset] & template;
            return result != 0;
        }
        public static void ClearAll(this Span<int> span)
        {
            span.Clear();
        }
        public static void SetBits(this Span<int> span, int startIndex, int endIndex, bool value)
        {
            // TODO: Optimize
            for (int i = startIndex; i < endIndex; i++)
            {
                span.SetBit(i, value);
            }
        }

        //// Excluding Index.
        //public static void ClearBefore(this Span<int> span, int index)
        //{
        //    if (index < 0)
        //    {
        //        return;
        //    }
        //    int intCount = index / 32;
        //    span.Slice(0, intCount).Fill(0);
        //    int remain = index % 32;
        //    for (int i = 0; i < remain; i++)
        //    {
        //        SetBit(span, intCount * 32 + i, false);
        //    }
        //}

        //// Including index.
        //public static void ClearAfter(this Span<int> span, int index)
        //{
        //    if (index < 0)
        //    {
        //        return;
        //    }
        //    int skipCount = index / 32 + 1;
        //    if (skipCount < span.Length)
        //    {
        //        span.Slice(skipCount).Fill(0);
        //    }

        //    int remain = 32 - (index % 32);
        //    for (int i = 0; i < remain; i++)
        //    {
        //        SetBit(span, skipCount * 32 - i - 1, false);
        //    }
        //}
    }

    public static class FrameExtensions
    {
        public static int GetSpanIndex(this IFrame frame, int x, int y)
        {
            if (x > frame.HorizontalResolution || y > frame.VerticalResolution || x < 0 || y < 0)
            {
                return -1;
            }
            else
            {
                return frame.HorizontalResolution * (frame.VerticalResolution - y - 1) + x;
            }
        }
    }

    internal static class CollectionExtensions
    {
        public static (TResult, TResult) MinMax<T, TResult>(this IEnumerable<T> array, Func<T, TResult> selector) where TResult : IComparable
        {
            TResult min = selector(array.First());
            TResult max = selector(array.First());

            foreach (var element in array)
            {
                var e = selector(element);
                if (e.CompareTo(min) < 0)
                {
                    min = e;
                }
                else if (e.CompareTo(max) > 0)
                {
                    max = e;
                }
            }

            return (min, max);
        }
    }
}
