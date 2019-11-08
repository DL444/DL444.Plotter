using DL444.Plotter.Library;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;

namespace DL444.Plotter.App.ViewModels
{
    internal class CircleViewModel : ViewModelBase
    {
        public CircleViewModel(CircleBase circle)
        {
            Circle = circle;
            _refEllipse = new Ellipse()
            {
                StrokeThickness = (double)Application.Current.Resources["ReferenceShapeStrokeThickness"],
                Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeSolidColorBrush"]
            };
            SetReferenceEllipseSizePosition(circle.Radius, circle.Center);
        }

        public override IGraphic Graphic => Circle;
        public override Shape ReferenceShape => _refEllipse;
        public override bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (IsSelected)
                {
                    _refEllipse.Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeHighlightSolidColorBrush"];
                    _refEllipse.StrokeThickness = (double)Application.Current.Resources["ReferenceShapeHighlightStrokeThickness"];
                }
                else
                {
                    _refEllipse.Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeSolidColorBrush"];
                    _refEllipse.StrokeThickness = (double)Application.Current.Resources["ReferenceShapeStrokeThickness"];
                }
            }
        }
        public CircleBase Circle { get; }

        public Point Center
        {
            get => Circle.Center;
            set
            {
                Circle.Center = value;
                NotifyPropertyChanged(nameof(Center));
                SetReferenceEllipseSizePosition(Radius, Center);
                NotifyPropertyChanged(nameof(ReferenceShape));
            }
        }
        public int Radius
        {
            get => Circle.Radius;
            set
            {
                Circle.Radius = value;
                NotifyPropertyChanged(nameof(Center));
                SetReferenceEllipseSizePosition(Radius, Center);
                NotifyPropertyChanged(nameof(ReferenceShape));
            }
        }

        private void SetReferenceEllipseSizePosition(int radius, Point center)
        {
            _refEllipse.Width = radius * 2;
            _refEllipse.Height = radius * 2;
            _refEllipse.Margin = new Thickness(center.X - radius, Circle.VerticalResolution - center.Y - radius - 1, 0, 0);
        }

        private Ellipse _refEllipse;
        private bool _isSelected;
    }
}
