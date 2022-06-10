using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        DebugViewModel debugViewModel;
        DebugWindow debugWindow;
        public MainWindow()
        {
            InitializeComponent();
            WPFSlideShowFactory factory = new WPFSlideShowFactory(MainGrid);
            debugViewModel = new DebugViewModel();
            debugWindow = new DebugWindow();
            debugWindow.DataContext = debugViewModel;
            
            DataContext = new SlideShowViewModel(factory, (string s) => debugViewModel.Text += $"{s}\n");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            debugWindow.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            debugWindow.Left = this.Left;
            debugWindow.Top = this.Top + this.Height;
            debugWindow.Show();
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
            tb.Margin = GetMargins(text, tb);
            grid.Children.Add(tb);
        }

        private static Thickness GetMargins(SlideText text, TextBlock tb)
        {

            Thickness thick = new Thickness(0);
            thick.Top = 2 * (900 - tb.FontSize) * (0.5 - text.YCoordinate);
            if (text.Justification == TextJustification.Left)
            {
                thick.Left = 1600 * text.LeftMargin;
            }
            else if(text.Justification == TextJustification.Right)
            {
                thick.Right = 1600 * text.RightMargin;
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
