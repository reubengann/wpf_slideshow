using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Show
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WPFSlideShowFactory factory = new WPFSlideShowFactory(MainGrid);
            DataContext = new SlideShowViewModel(factory);
        }
    }

    public interface SlideShowItemFactory
    {
        public void AddTextItem(SlideText text);
        public void RenderSlide(Slide slide);
    }

    public class WPFSlideShowFactory : SlideShowItemFactory
    {
        private Grid grid;

        public WPFSlideShowFactory(Grid mainGrid)
        {
            grid = mainGrid;
        }

        

        public void RenderSlide(Slide slide)
        {
            grid.Children.Clear();
            foreach(SlideItem si in slide.Items)
            {
                if (si.GetType() == typeof(SlideText))
                {
                    SlideText? t = si as SlideText;
                    if (t == null)
                        throw new NullReferenceException("Cannot add null text item");
                    AddTextItem(t);
                }
            }
        }

        public void AddTextItem(SlideText text)
        {
            var c = text.color;
            var tb = new TextBlock()
            {
                Text = text.Text,
                Foreground = new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = Application.Current.Resources["KarminaBoldItalic"] as FontFamily,
                FontSize = 100,
                TextAlignment=TextAlignment.Center
            };
            tb.Effect = new DropShadowEffect
            {
                ShadowDepth = 1
            };
            tb.Margin = new Thickness(0,  2 * (900 - tb.FontSize) * (text.YCoordinate - 0.5), 0, 0);
            grid.Children.Add(tb);
        }
    }

    public class ColorToSolidColorBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Color)
            {
                var c = (System.Drawing.Color)value;
                Color color = Color.FromArgb(c.A, c.R, c.G, c.B);
                return new SolidColorBrush(color);
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
