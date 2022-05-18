using System.Collections.Generic;
using System.Drawing;

namespace WpfApp1
{
    public class Slideshow
    {
        List<Slide> Slides;
        public Slideshow()
        {
            Slides = new List<Slide>();
        }
    }

    class Slide
    {
        Color BackgroundColor;
        List<SlideItem> Items;
    }

    class SlideItem
    {

    }

    class SlideText : SlideItem
    {
        string Text;
        Color color;
    }

    class SlideImage : SlideItem
    {
        string Path;

    }
}
