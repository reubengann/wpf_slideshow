﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
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
        public void AddImageItem(SlideImage image);
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
                    AddTextItem((SlideText)si);
                }
                else if(si.GetType() == typeof(SlideImage))
                {
                    AddImageItem((SlideImage)si);
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
            tb.Margin = GetTextMargin(text, tb);
            grid.Children.Add(tb);
        }

        private static Thickness GetTextMargin(SlideText text, TextBlock tb)
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

        public void AddImageItem(SlideImage image)
        {
            var ic = new Image();
            ic.Source = ToImageSource(image.image, image.image.RawFormat, image.scale);
            ic.Width = image.image.Width * image.scale;
            ic.Height = image.image.Height * image.scale;
            ic.Margin = GetImageMargin(image);
            if (image.crop != null)
            {
                Rect rect = GetCrop(image, (Thickness)image.crop);
                ic.Clip = new RectangleGeometry(rect);
            }

            if (image.rotation != 0)
            {
                if (image.crop != null)
                {
                    Rect rect = GetCrop(image, (Thickness)image.crop);
                    ic.RenderTransform = new RotateTransform(image.rotation, (rect.Width + rect.X) / 2, (rect.Height + rect.Y)/ 2);
                }
                else 
                    ic.RenderTransform = new RotateTransform(image.rotation, image.image.Width / 2, image.image.Height / 2);
            }
            grid.Children.Add(ic);
        }

        private Rect GetCrop(SlideImage image, Thickness thick)
        {
            Rect crop = new Rect();
            float w = (float)(1 - thick.Left - thick.Right);
            float h = (float)(1 - thick.Top - thick.Bottom);
            crop.X = (int)(thick.Left * image.scale * image.image.Width);
            crop.Y = (int)(thick.Top * image.scale * image.image.Height);
            crop.Width = (int)(image.image.Width * image.scale * w);
            crop.Height = (int)(image.image.Height * image.scale * h);
            return crop;
        }

        private Thickness GetImageMargin(SlideImage image)
        {
            // TODO: take margins into account
            var thick = new Thickness();
            thick.Top = 2 * (900) * (0.5 - image.y);
            thick.Left = 2 * (1600) * (image.x - 0.5);
            return thick;
        }

        private static ImageSource ToImageSource(System.Drawing.Image image, ImageFormat imageFormat, float scale)
        {
            BitmapImage bitmap = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                var dest = new System.Drawing.Bitmap(image, new System.Drawing.Size((int)(image.Width*scale), (int)(image.Height*scale)));
                
                dest.Save(stream, imageFormat);
                stream.Seek(0, SeekOrigin.Begin);
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            return bitmap;

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
