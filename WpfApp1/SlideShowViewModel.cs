using Prism.Commands;
using System;
using System.Drawing;
using System.Windows.Input;

namespace WpfApp1
{
    internal class SlideShowViewModel : BaseViewModel
    {
        private Slideshow slideshow;
        private SlideShowItemFactory factory;

        private int currentSlideIndex = 0;

        public ICommand NextSlide => new DelegateCommand(() => CurrentSlideIndex++);
        public ICommand PreviousSlide => new DelegateCommand(() => CurrentSlideIndex--);

        public int CurrentSlideIndex
        {
            get { return currentSlideIndex; }
            set 
            { 
                currentSlideIndex = value; 
                AdvanceSlide();
                OnPropertyChanged(nameof(CurrentSlideIndex));
            }
        }

        private Color currentBackgroundColor = Color.Green;

        public Color CurrentBackgroundColor
        {
            get { return currentBackgroundColor; }
            set { currentBackgroundColor = value; OnPropertyChanged(nameof(CurrentBackgroundColor)); }
        }


        private void AdvanceSlide()
        {
            if (currentSlideIndex < 0) currentSlideIndex = slideshow.Slides.Count - 1;
            if (currentSlideIndex > slideshow.Slides.Count - 1) currentSlideIndex = 0;
            RenderSlide();
        }

        public SlideShowViewModel(SlideShowItemFactory factory)
        {
            this.factory = factory;
            slideshow = new Slideshow();
            Slide a = new();
            a.Items.Add(new SlideText("Hello, Sailor!!!"));
            a.BackgroundColor = Color.Red;
            Slide b = new();
            b.Items.Add(new SlideText("There are 69,105 leaves in the pile."));
            b.BackgroundColor = Color.Green;
            Slide c = new();
            c.Items.Add(new SlideText("Unfortunately, there's a radio connected to my brain."));
            c.BackgroundColor = Color.Blue;
            Slide d = new();
            d.Items.Add(new SlideText("That's it."));
            d.BackgroundColor = Color.Purple;
            slideshow.Slides.Add(a);
            slideshow.Slides.Add(b);
            slideshow.Slides.Add(c);
            slideshow.Slides.Add(d);
            RenderSlide();
        }

        public void RenderSlide()
        {
            Slide slide = slideshow.Slides[currentSlideIndex];
            CurrentBackgroundColor = slide.BackgroundColor;
            factory.RenderSlide(slide);
        }
    }
}