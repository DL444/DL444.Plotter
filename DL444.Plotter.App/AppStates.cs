using DL444.Plotter.App.ViewModels;
using DL444.Plotter.Library;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace DL444.Plotter.App
{
    internal interface IAppState
    {
        string Description { get; }
        Brush CommandBarBrush { get; }
        Visibility AddButtonVisibility { get; }
        Visibility ClearButtonVisibility { get; }
        Visibility ConfirmButtonVisibility { get; }
        Visibility CancelButtonVisibility { get; }

        IAppState Add(GraphicType type);
        IAppState MouseOver(int x, int y);
        IAppState Click(int x, int y);
        IAppState Confirm();
        IAppState Cancel();
    }

    internal sealed class ReadyState : IAppState
    {
        public ReadyState(GridCanvas canvas, GraphicFactory factory)
        {
            this.canvas = canvas;
            this.factory = factory;
        }

        public string Description => "";
        public Brush CommandBarBrush => (Brush)Application.Current.Resources["SystemControlBackgroundChromeMediumBrush"];
        public Visibility AddButtonVisibility => Visibility.Visible;
        public Visibility ClearButtonVisibility => Visibility.Visible;
        public Visibility ConfirmButtonVisibility => Visibility.Collapsed;
        public Visibility CancelButtonVisibility => Visibility.Collapsed;

        public IAppState Add(GraphicType type)
        {
            switch (type)
            {
                case GraphicType.DdaLine:
                case GraphicType.MidpointLine:
                case GraphicType.BresenhamLine:
                    return new AddLineState(canvas, type, factory);
                case GraphicType.MidpointCircle:
                case GraphicType.BresenhamCircle:
                    return new AddCircleState(canvas, type, factory);
                case GraphicType.MidpointEllipse:
                    return new AddEllipseState(canvas, type, factory);
                case GraphicType.ScanlinePolygon:
                    return new AddPolygonState(canvas, type, factory);
                default:
                    throw new NotSupportedException($"The graphic type {type} is not yet supported.");
            }
        }
        public IAppState Cancel() => this;
        public IAppState Click(int x, int y) => this;
        public IAppState Confirm() => this;
        public IAppState MouseOver(int x, int y) => this;

        private GridCanvas canvas;
        private GraphicFactory factory;
    }

    internal sealed class AddLineState : IAppState
    {
        public AddLineState(GridCanvas canvas, GraphicType type, GraphicFactory factory)
        {
            this.canvas = canvas;
            previewLine = new Line()
            {
                StrokeThickness = 0.1,
                Stroke = (Brush)Application.Current.Resources["PreviewShapeColorBrush"],
                StrokeDashArray = new DoubleCollection() { 8.0 }
            };
            graphicType = type;
            this.factory = factory;
        }

        public string Description => $"Specify endpoint {(p0Set ? "B" : "A")}...";
        public Brush CommandBarBrush => (Brush)Application.Current.Resources["AddStateCommandBarColor"];
        public Visibility AddButtonVisibility => Visibility.Collapsed;
        public Visibility ClearButtonVisibility => Visibility.Collapsed;
        public Visibility ConfirmButtonVisibility => Visibility.Collapsed;
        public Visibility CancelButtonVisibility => Visibility.Visible;

        public Point Point0
        {
            get => point0;
            private set
            {
                point0 = value;
                previewLine.X1 = value.X;
                previewLine.Y1 = CoordinateHelper.GetMirroredY(value.Y, canvas.VerticalResolution);
            }
        }
        public Point Point1
        {
            get => point1;
            private set
            {
                point1 = value;
                previewLine.X2 = value.X;
                previewLine.Y2 = CoordinateHelper.GetMirroredY(value.Y, canvas.VerticalResolution);
            }
        }

        public IAppState Add(GraphicType type) => this;
        public IAppState Cancel()
        {
            canvas.PreviewShape = null;
            return new ReadyState(canvas, factory);
        }
        public IAppState Click(int x, int y)
        {
            if (p0Set)
            {
                Point1 = new Point(x, y);
                canvas.PreviewShape = null;
                var line = (LineSegmentViewModel)factory.GetGraphic(graphicType);
                line.Point0 = Point0;
                line.Point1 = Point1;
                canvas.Add(line);
                return new ReadyState(canvas, factory);
            }
            else
            {
                Point0 = new Point(x, y);
                p0Set = true;
                return this;
            }
        }
        public IAppState Confirm() => this;
        public IAppState MouseOver(int x, int y)
        {
            if (p0Set)
            {
                Point1 = new Point(x, y);
                if (!previewShapeAdded)
                {
                    canvas.PreviewShape = previewLine;
                    previewShapeAdded = true;
                }
            }
            else
            {
                Point0 = new Point(x, y);
            }
            return this;
        }

        private GridCanvas canvas;
        private Line previewLine;
        private bool p0Set;
        private bool previewShapeAdded;
        private Point point0 = new Point();
        private Point point1 = new Point();
        private GraphicType graphicType;
        private GraphicFactory factory;
    }

    internal sealed class AddCircleState : IAppState
    {
        public AddCircleState(GridCanvas canvas, GraphicType type, GraphicFactory factory)
        {
            this.canvas = canvas;
            previewEllipse = new Ellipse()
            {
                StrokeThickness = 0.1,
                Stroke = (Brush)Application.Current.Resources["PreviewShapeColorBrush"],
                StrokeDashArray = new DoubleCollection() { 8.0 }
            };
            graphicType = type;
            this.factory = factory;
        }

        public string Description => $"Specify {(centerSet ? "radius" : "center")}...";
        public Brush CommandBarBrush => (Brush)Application.Current.Resources["AddStateCommandBarColor"];
        public Visibility AddButtonVisibility => Visibility.Collapsed;
        public Visibility ClearButtonVisibility => Visibility.Collapsed;
        public Visibility ConfirmButtonVisibility => Visibility.Collapsed;
        public Visibility CancelButtonVisibility => Visibility.Visible;

        public Point Center 
        { 
            get => center;
            private set
            {
                center = value;
                SetPreviewEllipseSizePosition(Radius, Radius, Center);
            }
        }
        public int Radius
        {
            get => radius;
            private set
            {
                radius = value;
                SetPreviewEllipseSizePosition(Radius, Radius, Center);
            }
        }

        public IAppState Add(GraphicType type) => this;
        public IAppState Cancel()
        {
            canvas.PreviewShape = null;
            return new ReadyState(canvas, factory);
        }
        public IAppState Click(int x, int y)
        {
            if (centerSet)
            {
                Radius = GetRadius(Center, x, y);
                canvas.PreviewShape = null;
                var circle = (CircleViewModel)factory.GetGraphic(graphicType);
                circle.Center = Center;
                circle.Radius = Radius;
                canvas.Add(circle);
                return new ReadyState(canvas, factory);
            }
            else
            {
                Center = new Point(x, y);
                centerSet = true;
                return this;
            }
        }
        public IAppState Confirm() => this;
        public IAppState MouseOver(int x, int y)
        {
            if (centerSet)
            {
                Radius = GetRadius(Center, x, y);
                if (!previewShapeAdded)
                {
                    canvas.PreviewShape = previewEllipse;
                    previewShapeAdded = true;
                }
            }
            else
            {
                Center = new Point(x, y);
            }
            return this;
        }

        private void SetPreviewEllipseSizePosition(int a, int b, Point center)
        {
            previewEllipse.Width = a * 2;
            previewEllipse.Height = b * 2;
            previewEllipse.Margin = new Thickness(center.X - a, canvas.VerticalResolution - center.Y - b - 1, 0, 0);
        }
        private int GetRadius(Point center, int x, int y)
        {
            var d = (x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y);
            return (int)Math.Round(Math.Sqrt(d));
        }

        private GridCanvas canvas;
        private Ellipse previewEllipse;
        private bool centerSet;
        private bool previewShapeAdded;
        private Point center = new Point();
        private int radius = 0;
        private GraphicType graphicType;
        private GraphicFactory factory;
    }

    internal sealed class AddEllipseState : IAppState
    {
        public AddEllipseState(GridCanvas canvas, GraphicType type, GraphicFactory factory)
        {
            this.canvas = canvas;
            previewEllipse = new Ellipse()
            {
                StrokeThickness = 0.1,
                Stroke = (Brush)Application.Current.Resources["PreviewShapeColorBrush"],
                StrokeDashArray = new DoubleCollection() { 8.0 }
            };
            graphicType = type;
            this.factory = factory;
        }

        public string Description
        {
            get
            {
                if (aSet)
                {
                    return "Specify B...";
                }
                else if (centerSet)
                {
                    return "Specify A...";
                }
                else
                {
                    return "Specify center...";
                }
            }
        }
        public Brush CommandBarBrush => (Brush)Application.Current.Resources["AddStateCommandBarColor"];
        public Visibility AddButtonVisibility => Visibility.Collapsed;
        public Visibility ClearButtonVisibility => Visibility.Collapsed;
        public Visibility ConfirmButtonVisibility => Visibility.Collapsed;
        public Visibility CancelButtonVisibility => Visibility.Visible;

        public Point Center
        {
            get => center;
            private set
            {
                center = value;
                SetPreviewEllipseSizePosition(A, B, Center);
            }
        }
        public int A
        {
            get => a;
            private set
            {
                a = value;
                SetPreviewEllipseSizePosition(A, B, Center);
            }
        }
        public int B
        {
            get => b;
            private set
            {
                b = value;
                SetPreviewEllipseSizePosition(A, B, Center);
            }
        }


        public IAppState Add(GraphicType type) => this;
        public IAppState Cancel()
        {
            canvas.PreviewShape = null;
            return new ReadyState(canvas, factory);
        }
        public IAppState Click(int x, int y)
        {
            if (aSet)
            {
                B = Math.Abs(y - Center.Y);
                canvas.PreviewShape = null;
                var ellipse = (EllipseViewModel)factory.GetGraphic(graphicType);
                ellipse.Center = Center;
                ellipse.A = A;
                ellipse.B = B;
                canvas.Add(ellipse);
                return new ReadyState(canvas, factory);
            }
            else if (centerSet)
            {
                A = Math.Abs(x - Center.X);
                aSet = true;
                return this;
            }
            else
            {
                Center = new Point(x, y);
                centerSet = true;
                return this;
            }
        }
        public IAppState Confirm() => this;
        public IAppState MouseOver(int x, int y)
        {
            if (aSet)
            {
                B = Math.Abs(y - Center.Y);
            }
            else if (centerSet)
            {
                A = Math.Abs(x - Center.X);
                if (!previewShapeAdded)
                {
                    canvas.PreviewShape = previewEllipse;
                    previewShapeAdded = true;
                }
            }
            else
            {
                Center = new Point(x, y);
            }
            return this;
        }

        private void SetPreviewEllipseSizePosition(int a, int b, Point center)
        {
            previewEllipse.Width = a * 2;
            previewEllipse.Height = b * 2;
            previewEllipse.Margin = new Thickness(center.X - a, canvas.VerticalResolution - center.Y - b - 1, 0, 0);
        }

        private GridCanvas canvas;
        private Ellipse previewEllipse;
        private bool centerSet;
        private bool aSet;
        private bool previewShapeAdded;
        private Point center = new Point();
        private int a = 0;
        private int b = 0;
        private GraphicType graphicType;
        private GraphicFactory factory;
    }

    internal sealed class AddPolygonState : IAppState
    {
        public AddPolygonState(GridCanvas canvas, GraphicType type, GraphicFactory factory)
        {
            this.canvas = canvas;
            previewPolygon = new Polygon()
            {
                StrokeThickness = 0.1,
                Stroke = (Brush)Application.Current.Resources["PreviewShapeColorBrush"],
                StrokeDashArray = new DoubleCollection() { 8.0 },
                Points = new PointCollection()
            };
            this.factory = factory;
            polygon = (PolygonViewModel)factory.GetGraphic(type);
            canvas.PreviewShape = previewPolygon;
        }

        public string Description => $"Specify a point or confirm...";
        public Brush CommandBarBrush => (Brush)Application.Current.Resources["AddStateCommandBarColor"];
        public Visibility AddButtonVisibility => Visibility.Collapsed;
        public Visibility ClearButtonVisibility => Visibility.Collapsed;
        public Visibility ConfirmButtonVisibility => Visibility.Visible;
        public Visibility CancelButtonVisibility => Visibility.Visible;

        public IAppState Add(GraphicType type) => this;
        public IAppState Cancel()
        {
            canvas.PreviewShape = null;
            return new ReadyState(canvas, factory);
        }
        public IAppState Click(int x, int y)
        {
            polygon.Add(new Point(x, y));
            pSet = false;
            return this;
        }
        public IAppState Confirm()
        {
            canvas.PreviewShape = null;
            canvas.Add(polygon);
            return new ReadyState(canvas, factory);
        }
        public IAppState MouseOver(int x, int y)
        {
            if (!pSet)
            {
                pSet = true;
            }
            else
            {
                previewPolygon.Points.RemoveAt(previewPolygon.Points.Count - 1);
            }
            previewPolygon.Points.Add(new Windows.Foundation.Point(x, CoordinateHelper.GetMirroredY(y, canvas.VerticalResolution)));
            return this;
        }

        private GridCanvas canvas;
        private Polygon previewPolygon;
        private bool pSet;
        private GraphicFactory factory;
        private PolygonViewModel polygon;
    }

    internal sealed class CropLineState : IAppState
    {
        public CropLineState(GridCanvas canvas, GraphicFactory factory, LineSegmentViewModel line)
        {
            this.canvas = canvas;
            previewRect = new Rectangle()
            {
                StrokeThickness = 0.1,
                Stroke = (Brush)Application.Current.Resources["PreviewShapeColorBrush"],
                StrokeDashArray = new DoubleCollection() { 8.0 }
            };
            this.factory = factory;
            this.line = line;
        }

        public string Description => $"Specify window point {(p0Set ? "B" : "A")}...";
        public Brush CommandBarBrush => (Brush)Application.Current.Resources["AddStateCommandBarColor"];
        public Visibility AddButtonVisibility => Visibility.Collapsed;
        public Visibility ClearButtonVisibility => Visibility.Collapsed;
        public Visibility ConfirmButtonVisibility => Visibility.Collapsed;
        public Visibility CancelButtonVisibility => Visibility.Visible;

        public Point Point0
        {
            get => point0;
            private set
            {
                point0 = value;
                SetRectangleSizePosition(Point0, Point1);
            }
        }
        public Point Point1
        {
            get => point1;
            private set
            {
                point1 = value;
                SetRectangleSizePosition(Point0, Point1);
            }
        }

        public IAppState Add(GraphicType type) => this;
        public IAppState Cancel()
        {
            canvas.PreviewShape = null;
            return new ReadyState(canvas, factory);
        }
        public IAppState Click(int x, int y)
        {
            if (p0Set)
            {
                Point1 = new Point(x, y);
                canvas.PreviewShape = null;
                line.CropWindow = new CropWindow(Point0, Point1);
                canvas.Redraw();
                return new ReadyState(canvas, factory);
            }
            else
            {
                Point0 = new Point(x, y);
                Point1 = new Point(x, y);
                p0Set = true;
                return this;
            }
        }
        public IAppState Confirm() => this;
        public IAppState MouseOver(int x, int y)
        {
            if (p0Set)
            {
                Point1 = new Point(x, y);
                if (!previewShapeAdded)
                {
                    canvas.PreviewShape = previewRect;
                    previewShapeAdded = true;
                }
            }
            else
            {
                Point0 = new Point(x, y);
            }
            return this;
        }

        private void SetRectangleSizePosition(Point p0, Point p1)
        {
            int x = p1.X < p0.X ? p1.X : p0.X;
            int y = p1.Y > p0.Y ? p1.Y : p0.Y;
            y = CoordinateHelper.GetMirroredY(y, canvas.VerticalResolution);
            int height = Math.Abs(p0.Y - p1.Y);
            int width = Math.Abs(p0.X - p1.X);
            previewRect.Margin = new Thickness(x, y, 0, 0);
            previewRect.Height = height;
            previewRect.Width = width;
        }

        private GridCanvas canvas;
        private Rectangle previewRect;
        private bool p0Set;
        private bool previewShapeAdded;
        private Point point0 = new Point();
        private Point point1 = new Point();
        private GraphicFactory factory;
        private LineSegmentViewModel line;
    }
}
