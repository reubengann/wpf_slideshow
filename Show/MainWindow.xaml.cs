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
            var app = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var uri = new Uri(app, "./Fonts/Karmina Bold Italic.otf");
            FontLibrary.Instance.Add("karmina", uri);
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
                HorizontalAlignment = JustificationToHorizAlignment(text.Justification),
                VerticalAlignment = VerticalAlignment.Center,
                //FontFamily = Application.Current.Resources["KarminaBoldItalic"] as FontFamily,
                FontFamily = FontLibrary.Instance.Get(text.FontName),
                FontSize = 10 * text.FontSize,
                TextAlignment = JustificationToTextAlignment(text.Justification)
            };
            tb.Effect = new DropShadowEffect
            {
                ShadowDepth = 1
            };
            tb.Margin = GetThickness(text, tb);
            grid.Children.Add(tb);
        }

        private static Thickness GetThickness(SlideText text, TextBlock tb)
        {

            Thickness thick = new Thickness(0, 2 * (900 - tb.FontSize) * (0.5 - text.YCoordinate), 0, 0);
            if(text.Justification == TextJustification.Left)
            {
                thick.Left = 200;
            }
            else if(text.Justification == TextJustification.Right)
            {
                thick.Right = 200;
            }
            return thick;
        }

        static TextAlignment JustificationToTextAlignment(TextJustification j)
        {
            switch (j)
            {
                case TextJustification.Left: return TextAlignment.Left;
                case TextJustification.Right: return TextAlignment.Right;
                case TextJustification.Center: return TextAlignment.Center;
            }
            throw new Exception("Unknown justification type");
        }
        
        static HorizontalAlignment JustificationToHorizAlignment(TextJustification j)
        {
            switch (j)
            {
                case TextJustification.Left: return HorizontalAlignment.Left;
                case TextJustification.Right: return HorizontalAlignment.Right;
                case TextJustification.Center: return HorizontalAlignment.Center;
            }
            throw new Exception("Unknown justification type");
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
