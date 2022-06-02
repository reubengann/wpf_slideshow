using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Show
{
    public class SlideshowReader
    {
        private readonly string path;

        public SlideshowReader(string path)
        {
            this.path = path;
        }
        public Slideshow Load()
        {
            using StreamReader sr = new StreamReader(path);
            Slideshow slideshow = new Slideshow();
            string? line;
            Slide? CurrentSlide = null;
            int i = 0;
            while ((line = sr.ReadLine()) != null)
            {
                i++;
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith(':'))
                {
                    line = line[1..].TrimStart();
                    var (command, remainder) = BreakBySpaces(line);
                    switch(command)
                    {
                        case "slide":
                            if (!string.IsNullOrWhiteSpace(remainder)) Debug.WriteLine("Error: Junk after slide on line {0}", i);
                            CurrentSlide = new Slide();
                            slideshow.Slides.Add(CurrentSlide);
                            break;
                        default:
                            Debug.WriteLine("***************COMMAND {0}, RHS: {1}", command, remainder);
                            break;
                    }
                }
                else //text
                {
                    if(CurrentSlide == null)
                        Debug.WriteLine("Got text on line {0}, but no slide has been started", i);
                    else
                    {
                        CurrentSlide.Add(new SlideText(line));
                    }
                }
            }
            return slideshow;
        }

        private (string, string) BreakBySpaces(string line)
        {
            int SpacePos = line.IndexOf(' ');
            if (SpacePos == -1) return (line, "");
            string command = line[..SpacePos];
            return (command, line[SpacePos..].TrimStart());
        }
    }
}
