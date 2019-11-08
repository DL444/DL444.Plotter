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
            _refLine = new Line
            {
                StrokeThickness = (double)Application.Current.Resources["ReferenceShapeStrokeThickness"],
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
                _refLine.X1 = value.X;
                _refLine.Y1 = CoordinateHelper.GetMirroredY(value.Y, Graphic.VerticalResolution);
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
                _refLine.X2 = value.X;
                _refLine.Y2 = CoordinateHelper.GetMirroredY(value.Y, Graphic.VerticalResolution);
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

        public override Shape ReferenceShape => _refLine;
        public override bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (IsSelected)
                {
                    _refLine.Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeHighlightSolidColorBrush"];
                    _refLine.StrokeThickness = (double)Application.Current.Resources["ReferenceShapeHighlightStrokeThickness"];
                }
                else
                {
                    _refLine.Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeSolidColorBrush"];
                    _refLine.StrokeThickness = (double)Application.Current.Resources["ReferenceShapeStrokeThickness"];
                }
            }
        }

        private Line _refLine;
        private bool _isSelected;
    }
}
