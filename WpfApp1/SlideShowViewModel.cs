namespace WpfApp1
{
    internal class SlideShowViewModel : BaseViewModel
    {
        private Slideshow slideshow;
        private SlideShowItemFactory factory;

        public SlideShowViewModel(SlideShowItemFactory factory)
        {
            this.factory = factory;
            slideshow = new Slideshow();
            Slide slide = new Slide();
            var text = new SlideText("Hello, Sailor!!!");
            slide.Items.Add(text);
            slideshow.Slides.Add(slide);
            factory.AddTextItem(text);
        }
    }
}