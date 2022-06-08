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
        public Action<string>? OnLog;

        public SlideshowReader(string path)
        {
            this.path = path;
        }

        private void Log(string s)
        {
            OnLog?.Invoke(s);
        }

        public Slideshow Load()
        {
            Log($"Loading file {path}");
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
                            if (!string.IsNullOrWhiteSpace(remainder)) Log($"Error: Junk after slide on line {i}");
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
                                Log($"Error on line {i}: Expected four floats in background color, but got {e.Message}");
                            }
                            break;
                        case "y":
                            if (CurrentSlide == null) { PrintNoSlideError("text y coordinate", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            try
                            {
                                t.YCoordinate = float.Parse(remainder);
                            }
                            catch(FormatException e) { Log($"Error on line {i} while parsing y coordinate: Expected float, but got {e.Message}"); }
                            break;
                        case "text_color":
                            if (CurrentSlide == null) { PrintNoSlideError("text color", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            try
                            {
                                t.color = GetColorFromFloats(GetFourFloats(remainder));
                            }
                            catch (FormatException e) { Log($"Error on line {i} while parsing text color: Expected floats, but got {e.Message}"); }
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
                                    Log($"Invalid justification {remainder} on line {i}. Must be left, right or center.");
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
                                    Log($"Got invalid size {f} line {i}. Size must be positive.");
                                    continue;
                                }

                                t.FontSize = f;
                            }
                            catch (FormatException e) { Log($"Invalid size {remainder} on line {i}. Must be a float"); }
                            break;
                        case "declare_font":
                            var (fontName, fontFile) = BreakBySpaces(remainder);
                            var app = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location);
                            var uri = new Uri(app, $"./Fonts/{fontFile}");
                            try
                            {
                                FontLibrary.Instance.Add(fontName, uri);
                            }
                            catch (FileNotFoundException e) { Log($"Error loading {fontName} from {fontFile}: file not found."); }
                            break;
                        case "font":
                            if (CurrentSlide == null) { PrintNoSlideError("font", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            if (!FontLibrary.Instance.HasFont(remainder))
                            {
                                Log($"Could not find font {remainder}. Was it declared?");
                                continue;
                            }
                            t.FontName = remainder;
                            break;
                        default:
                            Log($"***************COMMAND {command}, RHS: {remainder}");
                            break;
                    }
                }
                else //text
                {
                    if(CurrentSlide == null)
                        Log($"Got text on line {i}, but no slide has been started");
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
            Log($"Got {command} on line {line}, but no slide has been started");
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
