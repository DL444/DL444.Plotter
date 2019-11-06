using DL444.Plotter.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;

namespace DL444.Plotter.App.ViewModels
{
    internal abstract class ViewModelBase : INotifyPropertyChanged
    {
        public abstract IGraphic Graphic { get; }

        public abstract Shape ReferenceShape { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
