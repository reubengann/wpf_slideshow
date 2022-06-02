using System.Collections.Generic;
using System.Drawing;

namespace Show
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
        public Color BackgroundColor = Color.Black;
        public List<SlideItem> Items;
        public Slide()
        {
            Items = new List<SlideItem>();
        }

        public void Add(SlideItem item) => Items.Add(item);
    }

    public class SlideItem
    {

    }

    public class SlideText : SlideItem
    {
        public string Text;
        public Color color = Color.White;

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
