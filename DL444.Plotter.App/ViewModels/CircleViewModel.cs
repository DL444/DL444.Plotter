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
            refEllipse = new Ellipse()
            {
                StrokeThickness = 0.1,
                Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeSolidColorBrush"]
            };
            SetReferenceEllipseSizePosition(circle.Radius, circle.Center);
        }

        public override IGraphic Graphic => Circle;
        public override Shape ReferenceShape => refEllipse;
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
            refEllipse.Width = radius * 2;
            refEllipse.Height = radius * 2;
            refEllipse.Margin = new Thickness(center.X - radius, Circle.VerticalResolution - center.Y - radius - 1, 0, 0);
        }

        private Ellipse refEllipse;
    }
}
