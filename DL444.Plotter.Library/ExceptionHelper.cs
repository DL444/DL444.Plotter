using System;

namespace DL444.Plotter.Library
{
    internal static class ExceptionHelper
    {
        public static void CheckSpanLength(ReadOnlySpan<int> span, int intOffset)
        {
            if (span.Length <= intOffset)
            {
                throw new IndexOutOfRangeException($"The specified index is longer than the span. " +
                    $"Byte offset: {intOffset}, Span length: {span.Length}");
            }
        }
        public static void CheckResolution(int hRes, int vRes)
        {
            if (hRes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hRes), "Horizontal Resolution cannot be negative.");
            }
            if (vRes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(vRes), "Vertical Resolution cannot be negative.");
            }
            if (hRes % 32 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hRes), "Horizontal Resolution must be divisible by 32.");
            }
            if (vRes % 32 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(vRes), "Vertical Resolution must be divisible by 32.");
            }
        }
    }
}
