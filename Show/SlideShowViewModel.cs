using Prism.Commands;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Show
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

        const string path = "../../../my.show";
        FileSystemWatcher watch;
        public SlideShowViewModel(SlideShowItemFactory factory)
        {
            this.factory = factory;
            SlideshowReader foobar = new SlideshowReader(path);
            slideshow = foobar.Load();
            RenderSlide();
            watch = new FileSystemWatcher();
            watch.Path = Path.GetDirectoryName(Path.GetFullPath(path));
            watch.Filter = Path.GetFileName(path);
            watch.Changed += FileChanged;
            watch.EnableRaisingEvents = true;
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            Task.Delay(100).ContinueWith(t => Application.Current.Dispatcher.Invoke(Reload));
        }

        private void Reload()
        {
            SlideshowReader foobar = new SlideshowReader(path);
            slideshow = foobar.Load();
            currentSlideIndex = Math.Min(currentSlideIndex, slideshow.Slides.Count - 1);
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