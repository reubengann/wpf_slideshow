using System.Collections.Generic;
using System.Drawing;

namespace WpfApp1
{
    public class Slideshow
    {
        public List<Slide> Slides;
        public Slideshow()
        {
            Slides = new List<Slide>();
        }
    }

    public class Slide
    {
        Color BackgroundColor;
        public List<SlideItem> Items;
        public Slide()
        {
            Items = new List<SlideItem>();
        }
    }

    public class SlideItem
    {

    }

    public class SlideText : SlideItem
    {
        public string Text;
        Color color;

        public SlideText(string text)
        {
            Text = text;
        }
    }

    class SlideImage : SlideItem
    {
        string Path;

    }
}
