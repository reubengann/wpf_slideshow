using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingTest
{
    public class DesignerCanvas : Canvas
    {

        public DesignerCanvas()
        {
            DesignerItem item = new DesignerItem();
            item.Content = new Ellipse() { Width = 100, Height = 100, Fill = new SolidColorBrush(Colors.Red), StrokeThickness = 2 };
            Children.Add(item);
        }
    }
}
