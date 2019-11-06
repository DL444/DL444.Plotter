﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DL444.Plotter.Library;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;

namespace DL444.Plotter.App.ViewModels
{
    internal class PolygonViewModel : ViewModelBase, IList<Point>
    {
        public PolygonViewModel(PolygonBase polygon)
        {
            Polygon = polygon;
            refPolygon = new Polygon()
            {
                StrokeThickness = 0.1,
                Stroke = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ReferenceShapeSolidColorBrush"],
                Points = new Windows.UI.Xaml.Media.PointCollection()
            };
            foreach (var p in polygon)
            {
                this.Add(CoordinateHelper.GetMirroredPoint(p, Graphic.VerticalResolution));
            }
        }

        public override IGraphic Graphic => Polygon;
        public override Shape ReferenceShape => refPolygon;
        public PolygonBase Polygon { get; }

        #region IList Implementation
        public int Count => ((IList<Point>)Polygon).Count;

        public bool IsReadOnly => ((IList<Point>)Polygon).IsReadOnly;

        public Point this[int index] { get => ((IList<Point>)Polygon)[index]; set => ((IList<Point>)Polygon)[index] = value; }

        public int IndexOf(Point item)
        {
            return ((IList<Point>)Polygon).IndexOf(item);
        }

        public void Insert(int index, Point item)
        {
            ((IList<Point>)Polygon).Insert(index, item);
            NotifyPropertyChanged(nameof(Graphic));
            NotifyPropertyChanged(nameof(Count));
            refPolygon.Points.Add(GetFrameworkPoint(CoordinateHelper.GetMirroredPoint(item, Graphic.VerticalResolution)));
            NotifyPropertyChanged(nameof(ReferenceShape));
        }

        public void RemoveAt(int index)
        {
            ((IList<Point>)Polygon).RemoveAt(index);
            NotifyPropertyChanged(nameof(Graphic));
            NotifyPropertyChanged(nameof(Count));
            refPolygon.Points.RemoveAt(index);
            NotifyPropertyChanged(nameof(ReferenceShape));
        }

        public void Add(Point item)
        {
            ((IList<Point>)Polygon).Add(item);
            NotifyPropertyChanged(nameof(Graphic));
            NotifyPropertyChanged(nameof(Count));
            refPolygon.Points.Add(GetFrameworkPoint(CoordinateHelper.GetMirroredPoint(item, Graphic.VerticalResolution)));
            NotifyPropertyChanged(nameof(ReferenceShape));
        }

        public void Clear()
        {
            ((IList<Point>)Polygon).Clear();
            NotifyPropertyChanged(nameof(Graphic));
            NotifyPropertyChanged(nameof(Count));
            refPolygon.Points.Clear();
            NotifyPropertyChanged(nameof(ReferenceShape));
        }

        public bool Contains(Point item)
        {
            return ((IList<Point>)Polygon).Contains(item);
        }

        public void CopyTo(Point[] array, int arrayIndex)
        {
            ((IList<Point>)Polygon).CopyTo(array, arrayIndex);
        }

        public bool Remove(Point item)
        {
            var result = ((IList<Point>)Polygon).Remove(item);
            if (result == true)
            {
                NotifyPropertyChanged(nameof(Graphic));
                NotifyPropertyChanged(nameof(Count));
                var y = CoordinateHelper.GetMirroredY(item.Y, Graphic.VerticalResolution);
                var refItem = refPolygon.Points.First(x => x.X == item.X && x.Y == y);
                refPolygon.Points.Remove(refItem);
                NotifyPropertyChanged(nameof(ReferenceShape));
            }
            return result;
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return ((IList<Point>)Polygon).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Point>)Polygon).GetEnumerator();
        }
        #endregion

        private Windows.Foundation.Point GetFrameworkPoint(Point point)
        {
            return new Windows.Foundation.Point(point.X, point.Y);
        }

        private Polygon refPolygon;
    }
}
