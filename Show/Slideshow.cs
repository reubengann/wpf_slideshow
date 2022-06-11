using System;
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
        public List<SlideItem> Items { get; private set; }
        public SlideText? CurrentSlideText { get; private set; } = null;
        public float RightMargin = 0.2f;
        public float LeftMargin = 0.2f;
        
        public Slide()
        {
            Items = new List<SlideItem>();
        }

        public void Add(SlideItem item)
        {
            if (item is SlideText)
            {
                CurrentSlideText = (SlideText)item;
                CurrentSlideText.LeftMargin = LeftMargin;
                CurrentSlideText.RightMargin = RightMargin;
                Items.Add(item);
            }
            else
            {
                Items.Add(item);
            }
        }

        public void CopyTemplate(Slide other)
        {
            this.BackgroundColor = other.BackgroundColor;
            this.Items = new List<SlideItem>(other.Items);
            this.LeftMargin = other.LeftMargin;
            this.RightMargin = other.RightMargin;
        }
    }

    public class SlideItem
    {

    }

    public class SlideText : SlideItem
    {
        public string Text;
        public Color color = Color.White;
        public float YCoordinate = 0.5f;
        public float FontSize = 10f;
        public TextJustification Justification = TextJustification.Center;
        public string FontName = "karmina";
        public float RightMargin = 0.2f;
        public float LeftMargin = 0.2f;


        public SlideText(string text)
        {
            Text = text;
        }

        public SlideText(SlideText t)
        {
            this.Text = t.Text;
            this.color = t.color;
            this.YCoordinate = t.YCoordinate;
            this.FontSize = t.FontSize;
            this.Justification = t.Justification;
            this.FontName = t.FontName;
        }

        public void PushText(string s)
        {
            if (string.IsNullOrEmpty(Text)) Text = s;
            else Text += $"\n{s}";
        }
    }

    public class SlideImage : SlideItem
    {
        public Image image;
        public float x = 0.5f;
        public float y = 0.5f;
        public float scale = 1f;

        public SlideImage(Image image)
        {
            this.image = image;
        }
    }

    public enum TextJustification
    {
        Center, Left, Right
    }
}
