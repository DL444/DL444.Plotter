using System;

namespace DL444.Plotter.Library
{
    public interface IFrame
    {
        int HorizontalResolution { get; }
        int VerticalResolution { get; }
        int PixelCount { get; }
        ReadOnlySpan<int> Frame { get; }
    }
}
