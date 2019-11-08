using DL444.Plotter.Library;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;

namespace DL444.Plotter.App.ViewModels
{
    internal class EllipseViewModel : ViewModelBase
    {
        public EllipseViewModel(EllipseBase ellipse)
        {
            Ellipse = ellipse;
            refEllipse = new Ellipse()
            {
                StrokeThickness = 0.1,
                Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeSolidColorBrush"]
            };
            SetReferenceEllipseSizePosition(Ellipse.A, Ellipse.B, Ellipse.Center);
        }

        public override IGraphic Graphic => Ellipse;
        public override Shape ReferenceShape => refEllipse;
        public EllipseBase Ellipse { get; }

        public int A
        {
            get => Ellipse.A;
            set
            {
                Ellipse.A = value;
                NotifyPropertyChanged(nameof(A));
                SetReferenceEllipseSizePosition(A, B, Center);
                NotifyPropertyChanged(nameof(ReferenceShape));
            }
        }
        public int B
        {
            get => Ellipse.B;
            set
            {
                Ellipse.B = value;
                NotifyPropertyChanged(nameof(B));
                SetReferenceEllipseSizePosition(A, B, Center);
                NotifyPropertyChanged(nameof(ReferenceShape));
            }
        }
        public Point Center
        {
            get => Ellipse.Center;
            set
            {
                Ellipse.Center = value;
                NotifyPropertyChanged(nameof(Center));
                SetReferenceEllipseSizePosition(A, B, Center);
                NotifyPropertyChanged(nameof(ReferenceShape));
            }
        }

        private void SetReferenceEllipseSizePosition(int a, int b, Point center)
        {
            refEllipse.Width = a * 2;
            refEllipse.Height = b * 2;
            refEllipse.Margin = new Thickness(center.X - a, Ellipse.VerticalResolution - center.Y - b - 1, 0, 0);
        }

        private Ellipse refEllipse;
    }
}
