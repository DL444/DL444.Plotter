using DL444.Plotter.App.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DL444.Plotter.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            this.InitializeComponent();
            factory = new GraphicFactory(HorizontalResolution, VerticalResolution);
            AppState = new ReadyState(canvas, factory);
        }

        private int HorizontalResolution { get; } = 64;
        private int VerticalResolution { get; } = 64;
        private IAppState AppState 
        { 
            get => _appState;
            set
            {
                _appState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AppState)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Canvas_HoverChanged(object sender, GridCanvasPointerEventArgs e)
        {
            AppState = AppState.MouseOver(e.X, e.Y);
        }
        private void Canvas_Clicked(object sender, GridCanvasPointerEventArgs e)
        {
            AppState = AppState.Click(e.X, e.Y);
        }
        private void AddGraphic_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuFlyoutItem)sender;
            var tag = int.Parse((string)item.Tag);
            var graphicType = (GraphicType)tag;
            AppState = AppState.Add(graphicType);
        }
        private void DeleteGraphic_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuFlyoutItem)sender;
            var tag = (ViewModelBase)item.Tag;
            canvas.Remove(tag);
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            ((Flyout)btn.Tag).Hide();
            canvas.Clear();
        }
        private void ConfirmAddition_Click(object sender, RoutedEventArgs e)
        {
            AppState = AppState.Confirm();
        }
        private void CancelAddition_Click(object sender, RoutedEventArgs e)
        {
            AppState = AppState.Cancel();
        }
        private void LineSegmentCrop_Click(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var tag = (LineSegmentViewModel)item.Tag;
            AppState.Cancel();
            AppState = new CropLineState(canvas, factory, tag);
        }
        private void LineSegmentUncrop_Click(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var tag = (LineSegmentViewModel)item.Tag;
            tag.CropWindow = null;
            canvas.Redraw();
        }

        private IAppState _appState;
        private GraphicFactory factory;
    }

    public class GraphicDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case LineSegmentViewModel _:
                    return GetTemplate("LineSegmentDataTemplate");
                case CircleViewModel _:
                    return GetTemplate("CircleDataTemplate");
                case EllipseViewModel _:
                    return GetTemplate("EllipseDataTemplate");
                case PolygonViewModel _:
                    return GetTemplate("PolygonDataTemplate");
                default:
                    return base.SelectTemplateCore(item);
            }
        }

        private DataTemplate GetTemplate(string key)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (Page)frame.Content;
            return (DataTemplate)page.Resources[key];
        }
    }
    public class BooleanInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }
    }
}
