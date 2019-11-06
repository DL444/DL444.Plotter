using System;

namespace DL444.Plotter.Library
{
    public interface IGraphic : IFrame
    {
        Guid Id { get; }
        bool Dirty { get; }
        void Draw();
        void ClearFrame();
    }
}
