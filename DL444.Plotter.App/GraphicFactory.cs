using DL444.Plotter.App.ViewModels;
using DL444.Plotter.Library;
using System;

namespace DL444.Plotter.App
{
    internal class GraphicFactory
    {
        public GraphicFactory(int hRes, int vRes)
        {
            HorizontalResolution = hRes;
            VerticalResolution = vRes;
        }

        public int HorizontalResolution { get; }
        public int VerticalResolution { get; }

        public ViewModelBase GetGraphic(GraphicType type)
        {
            switch (type)
            {
                case GraphicType.DdaLine:
                    return new LineSegmentViewModel(new DdaLineSegment(HorizontalResolution, VerticalResolution));
                case GraphicType.MidpointLine:
                    return new LineSegmentViewModel(new MidpointLineSegment(HorizontalResolution, VerticalResolution));
                case GraphicType.BresenhamLine:
                    return new LineSegmentViewModel(new BresenhamLineSegment(HorizontalResolution, VerticalResolution));
                case GraphicType.MidpointCircle:
                    return new CircleViewModel(new MidpointCircle(HorizontalResolution, VerticalResolution));
                case GraphicType.BresenhamCircle:
                    return new CircleViewModel(new BresenhamCircle(HorizontalResolution, VerticalResolution));
                case GraphicType.MidpointEllipse:
                    return new EllipseViewModel(new MidpointEllipse(HorizontalResolution, VerticalResolution));
                case GraphicType.ScanlinePolygon:
                    return new PolygonViewModel(new ScanlinePolygon(HorizontalResolution, VerticalResolution));
                default:
                    throw new NotSupportedException($"Graphic type {type} is not yet supported.");
            }
        }
    }

    internal enum GraphicType
    {
        Point = 00,
        DdaLine = 10, MidpointLine = 11, BresenhamLine = 12,
        MidpointCircle = 20, BresenhamCircle = 21,
        MidpointEllipse = 30,
        ScanlinePolygon = 40,
        GraphicsGroup = 50
    }
}
