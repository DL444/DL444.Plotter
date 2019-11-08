using DL444.Plotter.Library;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;

namespace DL444.Plotter.App.ViewModels
{
    internal class LineSegmentViewModel : ViewModelBase
    {
        public LineSegmentViewModel(LineSegmentBase lineSeg)
        {
            LineSegment = lineSeg;
            refLine = new Line
            {
                StrokeThickness = 0.1,
                Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeSolidColorBrush"],
                X1 = lineSeg.Point0.X,
                Y1 = CoordinateHelper.GetMirroredY(lineSeg.Point0.Y, Graphic.VerticalResolution),
                X2 = lineSeg.Point1.X,
                Y2 = CoordinateHelper.GetMirroredY(lineSeg.Point1.Y, Graphic.VerticalResolution)
            };
        }

        public override IGraphic Graphic => LineSegment;
        public LineSegmentBase LineSegment { get; }

        public Point Point0
        {
            get => LineSegment.Point0;
            set
            {
                LineSegment.Point0 = value;
                NotifyPropertyChanged(nameof(Point0));
                refLine.X1 = value.X;
                refLine.Y1 = CoordinateHelper.GetMirroredY(value.Y, Graphic.VerticalResolution);
                NotifyPropertyChanged(nameof(ReferenceShape));
            }
        }
        public Point Point1
        {
            get => LineSegment.Point1;
            set
            {
                LineSegment.Point1 = value;
                NotifyPropertyChanged(nameof(Point1));
                refLine.X2 = value.X;
                refLine.Y2 = CoordinateHelper.GetMirroredY(value.Y, Graphic.VerticalResolution);
                NotifyPropertyChanged(nameof(ReferenceShape));
            }
        }
        public CropWindow? CropWindow
        {
            get => LineSegment.CropWindow;
            set
            {
                LineSegment.CropWindow = value;
                NotifyPropertyChanged(nameof(CropWindow));
                Cropped = value != null;
                NotifyPropertyChanged(nameof(Cropped));
            }
        }
        public bool Cropped { get; private set; }

        public override Shape ReferenceShape => refLine;

        private Line refLine;
    }
}
