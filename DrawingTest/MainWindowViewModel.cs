using Prism.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingTest
{
    public class MainWindowViewModel : BaseViewModel
    {
        private UIElementCollection Canvas;

        public MainWindowViewModel(UIElementCollection children)
        {
            this.Canvas = children;
        }

        public ICommand AddEllipseCommand => new DelegateCommand(AddEllipse);
        public ICommand AddRectangleCommand => new DelegateCommand(AddRectangle);

        private void AddRectangle()
        {
            DesignerItem item = new DesignerItem() { Width = 100, Height = 100 };
            item.Content = new Rectangle() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 2, IsHitTestVisible = false };
            Canvas.Add(item);
        }

        private void AddEllipse()
        {
            DesignerItem item = new DesignerItem() { Width = 100, Height = 100 };
            item.Content = new Ellipse() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 2, IsHitTestVisible = false };
            Canvas.Add(item);
        }
    }

    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
