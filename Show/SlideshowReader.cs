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
            SlideText t = new SlideText("");
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
                            if (CurrentSlide == null) { PrintNoSlideError("backgound color", i); continue; };
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
                            if (CurrentSlide == null) { PrintNoSlideError("text y coordinate", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            try
                            {
                                t.YCoordinate = float.Parse(remainder);
                            }
                            catch(FormatException e) { Debug.WriteLine("Error on line {0}: Expected floats, but got {1}", i, e.Message); }
                            break;
                        case "text_color":
                            if (CurrentSlide == null) { PrintNoSlideError("text color", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            try
                            {
                                t.color = GetColorFromFloats(GetFourFloats(remainder));
                            }
                            catch (FormatException e) { Debug.WriteLine("Error on line {0}: Expected floats, but got {1}", i, e.Message); }
                            break;
                        case "justify":
                            if (CurrentSlide == null) { PrintNoSlideError("justification", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
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
                            if (CurrentSlide == null) { PrintNoSlideError("size", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            try
                            {
                                float f = float.Parse(remainder);
                                if (f < 0)
                                {
                                    Debug.WriteLine("Got invalid size {1} line {0}. Size must be positive.", i, f);
                                    continue;
                                }

                                t.FontSize = f;
                            }
                            catch (FormatException e) { 
                                    Debug.WriteLine("Invalid size {0} on line {1}. Must be a float", remainder, i);
                            }
                            break;
                        case "declare_font":
                            var (fontName, fontFile) = BreakBySpaces(remainder);
                            var app = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location);
                            var uri = new Uri(app, $"./Fonts/{fontFile}");
                            try
                            {
                                FontLibrary.Instance.Add(fontName, uri);
                            }
                            catch (FileNotFoundException e) { Debug.WriteLine("Error loading {0}. Was it declared?", fontName); }
                            break;
                        case "font":
                            if (CurrentSlide == null) { PrintNoSlideError("font", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            if (!FontLibrary.Instance.HasFont(remainder))
                            {
                                Debug.WriteLine("Could not find font {0}", remainder);
                                continue;
                            }
                            t.FontName = remainder;
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

        private void PrintNoSlideError(string command, int line)
        {
            Debug.WriteLine("Got {1} on line {0}, but no slide has been started", line, command);
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
