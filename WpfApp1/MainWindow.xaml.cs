using System;
using System.Collections.Generic;
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

namespace WpfApp1
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
    }

    public class WPFSlideShowFactory : SlideShowItemFactory
    {
        private Grid grid;

        public WPFSlideShowFactory(Grid mainGrid)
        {
            grid = mainGrid;
        }

        public void AddTextItem(SlideText text)
        {
            var tb = new TextBlock()
            {
                Text = text.Text,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = Application.Current.Resources["KarminaBoldItalic"] as FontFamily,
                FontSize = 100
            };
            tb.Effect = new DropShadowEffect
            {
                ShadowDepth = 1
            };
            grid.Children.Add(tb);
        }
    }
}
