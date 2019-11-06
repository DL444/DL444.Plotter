using DL444.Plotter.App.ViewModels;
using DL444.Plotter.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DL444.Plotter.App
{
    internal sealed partial class GridCanvas : UserControl, IList<ViewModelBase>
    {
        public GridCanvas()
        {
            this.InitializeComponent();
        }
        public GridCanvas(int hRes, int vRes) : this()
        {
            InitCanvas(hRes, vRes);
        }

        public int HorizontalResolution
        {
            get => (int)GetValue(HorizontalResolutionProperty);
            set
            {
                SetValue(HorizontalResolutionProperty, value);
                InitCanvas(HorizontalResolution, VerticalResolution);
            }
        }
        public int VerticalResolution
        {
            get => (int)GetValue(VerticalResolutionProperty);
            set
            {
                SetValue(VerticalResolutionProperty, value);
                InitCanvas(HorizontalResolution, VerticalResolution);
            }
        }
        public Shape PreviewShape
        {
            get => _previewShape;
            set
            {
                if (_previewShape != null)
                {
                    baseCanvas.Children.Remove(_previewShape);
                }
                _previewShape = value;
                if (value != null)
                {
                    baseCanvas.Children.Add(value);
                }
            }
        }

        public ObservableCollection<ViewModelBase> ViewModels { get; } = new ObservableCollection<ViewModelBase>();

        public void Redraw()
        {
            composer.TriggerDraw();
        }

        public static readonly DependencyProperty HorizontalResolutionProperty =
            DependencyProperty.Register(nameof(HorizontalResolution), typeof(int), typeof(GridCanvas), new PropertyMetadata(0));
        public static readonly DependencyProperty VerticalResolutionProperty =
            DependencyProperty.Register(nameof(VerticalResolution), typeof(int), typeof(GridCanvas), new PropertyMetadata(0));

        public event EventHandler<GridCanvasPointerEventArgs> Clicked;
        public event EventHandler<GridCanvasPointerEventArgs> HoverChanged;

        #region IList Implementation
        public int Count => ((IList<ViewModelBase>)ViewModels).Count;

        public bool IsReadOnly => ((IList<ViewModelBase>)ViewModels).IsReadOnly;

        public ViewModelBase this[int index] { get => ((IList<ViewModelBase>)ViewModels)[index]; set => ((IList<ViewModelBase>)ViewModels)[index] = value; }

        public int IndexOf(ViewModelBase item)
        {
            return ((IList<ViewModelBase>)ViewModels).IndexOf(item);
        }

        public void Insert(int index, ViewModelBase item)
        {
            ((IList<ViewModelBase>)ViewModels).Insert(index, item);
            baseCanvas.Children.Insert(index, item.ReferenceShape);
            composer.Insert(index, item.Graphic);
            composer.TriggerDraw();
        }

        public void RemoveAt(int index)
        {
            var item = ViewModels[index];
            ((IList<ViewModelBase>)ViewModels).RemoveAt(index);
            baseCanvas.Children.Remove(item.ReferenceShape);
            composer.RemoveAt(index);
            composer.TriggerDraw();
        }

        public void Add(ViewModelBase item)
        {
            ((IList<ViewModelBase>)ViewModels).Add(item);
            baseCanvas.Children.Add(item.ReferenceShape);
            composer.Add(item.Graphic);
            composer.TriggerDraw();
        }

        public void Clear()
        {
            foreach (var vm in ViewModels)
            {
                baseCanvas.Children.Remove(vm.ReferenceShape);
            }
            ((IList<ViewModelBase>)ViewModels).Clear();
            composer.Clear();
            composer.TriggerDraw();
        }

        public bool Contains(ViewModelBase item)
        {
            return ((IList<ViewModelBase>)ViewModels).Contains(item);
        }

        public void CopyTo(ViewModelBase[] array, int arrayIndex)
        {
            ((IList<ViewModelBase>)ViewModels).CopyTo(array, arrayIndex);
        }

        public bool Remove(ViewModelBase item)
        {
            bool result = ((IList<ViewModelBase>)ViewModels).Remove(item);
            if (result == true)
            {
                baseCanvas.Children.Remove(item.ReferenceShape);
                composer.Remove(item.Graphic);
                composer.TriggerDraw();
            }
            return result;
        }

        public IEnumerator<ViewModelBase> GetEnumerator()
        {
            return ((IList<ViewModelBase>)ViewModels).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<ViewModelBase>)ViewModels).GetEnumerator();
        }
        #endregion

        private void InitCanvas(int hRes, int vRes)
        {
            baseCanvas.Children.Clear();

            if (hRes <= 0 || vRes <= 0)
            {
                baseCanvas.Width = 0;
                baseCanvas.Height = 0;
                return;
            }

            composer = new Composer(hRes, vRes, DrawPoint);
            ViewModels.Clear();
            points = new Ellipse[hRes, vRes];

            baseCanvas.Width = hRes - 1;
            baseCanvas.Height = vRes - 1;

            PrepareGrid(hRes, vRes);
        }
        private void BaseCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var location = e.GetPosition(baseCanvas);
            (int x, int y) = ConvertCoordinate(location.X, location.Y);
            Clicked?.Invoke(this, new GridCanvasPointerEventArgs(x, y));
        }
        private void BaseCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var location = e.GetCurrentPoint(baseCanvas).Position;
            (int x, int y) = ConvertCoordinate(location.X, location.Y);
            if (x != lastX || y != lastY)
            {
                lastX = x;
                lastY = y;
                HoverChanged?.Invoke(this, new GridCanvasPointerEventArgs(x, y));
            }
        }
        private (int, int) ConvertCoordinate(double x, double y)
        {
            int resultX = (int)Math.Round(x);
            int resultY = (int)Math.Round(y);
            resultY = VerticalResolution - resultY - 1;
            return (resultX, resultY);
        }
        private void PrepareGrid(int hRes, int vRes)
        {
            for (int i = 0; i < hRes; i++)
            {
                Line ln = new Line
                {
                    Stroke = (Brush)Application.Current.Resources["GridSolidColorBrush"],
                    StrokeThickness = 0.1,
                    X1 = i,
                    Y1 = 0,
                    X2 = i,
                    Y2 = VerticalResolution - 1
                };
                baseCanvas.Children.Add(ln);
            }
            for (int i = 0; i < vRes; i++)
            {
                Line ln = new Line
                {
                    Stroke = (Brush)Application.Current.Resources["GridSolidColorBrush"],
                    StrokeThickness = 0.1,
                    X1 = 0,
                    Y1 = i,
                    X2 = HorizontalResolution - 1,
                    Y2 = i
                };
                baseCanvas.Children.Add(ln);
            }
            for (int i = 0; i < hRes; i++)
            {
                for (int j = 0; j < vRes; j++)
                {
                    points[i, j] = new Ellipse()
                    {
                        StrokeThickness = 0,
                        Fill = (Brush)Application.Current.Resources["PointSolidColorBrush"],
                        Height = 0.5,
                        Width = 0.5,
                        Visibility = Visibility.Collapsed
                    };
                    Canvas.SetLeft(points[i, j], i - 0.25);
                    Canvas.SetTop(points[i, j], vRes - j - 1.25);
                    baseCanvas.Children.Add(points[i, j]);
                }
            }
        }
        private void DrawPoint(int x, int y, bool value)
        {
            points[x, VerticalResolution - 1 - y].Visibility = value == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private int lastX = 0;
        private int lastY = 0;
        private Composer composer;
        private Ellipse[,] points;
        private Shape _previewShape;
    }

    internal class GridCanvasPointerEventArgs : EventArgs, IEquatable<GridCanvasPointerEventArgs>
    {
        public GridCanvasPointerEventArgs() { }
        public GridCanvasPointerEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is GridCanvasPointerEventArgs e)
            {
                return Equals(e);
            }
            return false;
        }
        public bool Equals(GridCanvasPointerEventArgs other)
        {
            return other != null &&
                   X == other.X &&
                   Y == other.Y;
        }
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(GridCanvasPointerEventArgs left, GridCanvasPointerEventArgs right)
        {
            return EqualityComparer<GridCanvasPointerEventArgs>.Default.Equals(left, right);
        }
        public static bool operator !=(GridCanvasPointerEventArgs left, GridCanvasPointerEventArgs right)
        {
            return !(left == right);
        }
    }
}
