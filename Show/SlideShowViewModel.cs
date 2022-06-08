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
        public ICommand LastSlide => new DelegateCommand(() => CurrentSlideIndex = slideshow.Slides.Count - 1);
        public ICommand FirstSlide => new DelegateCommand(() => CurrentSlideIndex = 0);

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
        Action<string>? Log;
        public SlideShowViewModel(SlideShowItemFactory factory, Action<string>? log = null)
        {
            this.factory = factory;
            SlideshowReader foobar = new SlideshowReader(path);
            Log = log;
            foobar.OnLog += LogMessage;
            slideshow = foobar.Load();
            foobar.OnLog -= LogMessage;
            RenderSlide();
            watch = new FileSystemWatcher();
            watch.Path = Path.GetDirectoryName(Path.GetFullPath(path));
            watch.Filter = Path.GetFileName(path);
            watch.Changed += FileChanged;
            watch.NotifyFilter = NotifyFilters.LastWrite;
            watch.EnableRaisingEvents = true;
        }
        private void LogMessage(string s)
        {
            Log?.Invoke(s);
        }

        private bool HandlingNow = false;
        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            if (HandlingNow) return;
            HandlingNow = true;
            Task.Delay(100).ContinueWith(t => Application.Current.Dispatcher.Invoke(Reload));
        }

        private void Reload()
        {
            LogMessage("Detected file changes. Reloading");
            SlideshowReader foobar = new SlideshowReader(path);
            foobar.OnLog += LogMessage;
            slideshow = foobar.Load();
            foobar.OnLog -= LogMessage;
            currentSlideIndex = Math.Min(currentSlideIndex, slideshow.Slides.Count - 1);
            RenderSlide();
            HandlingNow = false;
        }

        public void RenderSlide()
        {
            Slide slide = slideshow.Slides[currentSlideIndex];
            CurrentBackgroundColor = slide.BackgroundColor;
            factory.RenderSlide(slide);
        }
    }
}