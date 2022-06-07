using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
            SlideText? t = null;
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
                        case "background":
                            if(CurrentSlide == null)
                            {
                                Debug.WriteLine("Got backgound color on line {0}, but no slide has been started", i);
                                continue;
                            }
                            try
                            {
                                float[] FracColors = GetFourFloats(remainder);
                                Color background = GetColorFromFloats(FracColors);
                                CurrentSlide.BackgroundColor = background;
                            }
                            catch(FormatException e) 
                            {
                                Debug.WriteLine("Error on line {0}: Expected four floats in background color, but got {1}", i, e.Message);
                            }
                            break;
                        case "y":
                                if (CurrentSlide == null)
                                {
                                    Debug.WriteLine("Got text y coordinate on line {0}, but no slide has been started", i);
                                    continue;
                                }
                                if (t == null)
                                    t = new SlideText("");
                            try 
                            {
                                t.YCoordinate = float.Parse(remainder);
                            }
                            catch(FormatException e) { Debug.WriteLine("Error on line {0}: Expected floats, but got {1}", i, e.Message); }
                            break;
                        case "text_color":
                                if (CurrentSlide == null)
                                {
                                    Debug.WriteLine("Got text color on line {0}, but no slide has been started", i);
                                    continue;
                                }
                                if (t == null)
                                    t = new SlideText("");
                            try
                            {
                                t.color = GetColorFromFloats(GetFourFloats(remainder));
                            }
                            catch (FormatException e) { Debug.WriteLine("Error on line {0}: Expected floats, but got {1}", i, e.Message); }
                            break;
                        case "justify":
                            if (CurrentSlide == null)
                            {
                                Debug.WriteLine("Got justification command on line {0}, but no slide has been started", i);
                                continue;
                            }
                            if (t == null)
                                t = new SlideText("");
                            switch (remainder)
                            {
                                case "left":
                                    t.Justification = TextJustification.Left;
                                    break;
                                case "right":
                                    t.Justification = TextJustification.Right;
                                    break;
                                case "center":
                                    t.Justification = TextJustification.Center;
                                    break;
                                default:
                                    Debug.WriteLine("Invalid justification {0} on line {1}. Must be left, right or center.", remainder, i);
                                    break;
                            }
                            
                            break;
                        case "size":
                            if (CurrentSlide == null)
                            {
                                Debug.WriteLine("Got size command on line {0}, but no slide has been started", i);
                                continue;
                            }
                            if (t == null)
                                t = new SlideText("");
                            try
                            {
                                t.FontSize = float.Parse(remainder);
                            }
                            catch (FormatException e) { 
                                    Debug.WriteLine("Invalid size {0} on line {1}. Must be a float", remainder, i);
                            }
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
                        if(CurrentSlide.CurrentSlideText == null)
                        {
                            t.Text = line;
                            CurrentSlide.Add(t);
                            t = new SlideText("");
                        }
                        else
                        {
                            CurrentSlide.CurrentSlideText.PushText(line);
                        }
                    }
                }
            }
            return slideshow;
        }


        private Color GetColorFromFloats(float[] fracColors)
        {
            int[] c = fracColors.Select(x => (int)(x * 255)).ToArray();
            return Color.FromArgb(c[3], c[0], c[1], c[2]);
        }

        private float[] GetFourFloats(string remainder)
        {
            float[] result = remainder.Split().Select(x => float.Parse(x)).ToArray();
            if (result.Length != 4) throw new FormatException();
            return result;
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
