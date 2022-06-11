﻿using System;
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
        Dictionary<string, Image> images;

        public SlideshowReader(string path)
        {
            this.path = path;
            images = new();
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
            bool continuingText = false;
            string definingStyleName = "";
            Dictionary<string, SlideText> styles = new();
            Dictionary<string, Slide> templates = new();
            
            while ((line = sr.ReadLine()) != null)
            {
                i++;
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith(':'))
                {
                    line = line[1..].TrimStart();
                    var (command, remainder) = BreakBySpaces(line);
                    switch (command)
                    {
                        case "slide":
                            if(!string.IsNullOrEmpty(definingStyleName))
                            {
                                Log($"Error on line {i}: Attempt to declare slide while defining style {definingStyleName}.");
                                definingStyleName = "";
                            }
                            string slideName = "";
                            bool visible = true;
                            if (!string.IsNullOrEmpty(remainder))
                            {
                                var (attemptedslideName, visibilityName) = BreakBySpaces(remainder);
                                switch(visibilityName)
                                {
                                    case "yes": visible = true; break;
                                    case "no": visible = false; break;
                                    case "": visible = true; break;
                                    default: 
                                        Log($"Error on line {i}: Invalid visibility {visibilityName} (must be yes or no or leaving it blank)");
                                        break;
                                }
                                if(templates.ContainsKey(attemptedslideName))
                                {
                                    Log($"Error on line {i}: attempt to redefine slide {attemptedslideName}");
                                    continue;
                                }
                                slideName = attemptedslideName;
                            }
                            CurrentSlide = new Slide();
                            if(visible)
                                slideshow.Slides.Add(CurrentSlide);
                            if (!string.IsNullOrEmpty(slideName))
                                templates[slideName] = CurrentSlide;
                            t = new SlideText("");
                            break;
                        case "background":
                            if (CurrentSlide == null) { PrintNoSlideError("backgound color", i); continue; };
                            try
                            {
                                float[] FracColors = GetFourFloats(remainder);
                                Color background = GetColorFromFloats(FracColors);
                                CurrentSlide.BackgroundColor = background;
                            }
                            catch (FormatException e)
                            {
                                Log($"Error on line {i}: Expected four floats in background color, but got {e.Message}");
                            }
                            break;
                        case "y":
                            if (CurrentSlide == null && string.IsNullOrEmpty(definingStyleName)) { PrintNoSlideError("text y coordinate", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            try
                            {
                                t.YCoordinate = float.Parse(remainder);
                            }
                            catch (FormatException e) { Log($"Error on line {i} while parsing y coordinate: Expected float, but got {e.Message}"); }
                            continuingText = false;
                            break;
                        case "text_color":
                            if (CurrentSlide == null && string.IsNullOrEmpty(definingStyleName)) { PrintNoSlideError("text color", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            try
                            {
                                t.color = GetColorFromFloats(GetFourFloats(remainder));
                            }
                            catch (FormatException e) { Log($"Error on line {i} while parsing text color: Expected floats, but got {e.Message}"); }
                            continuingText = false;
                            break;
                        case "justify":
                            if (CurrentSlide == null && string.IsNullOrEmpty(definingStyleName)) { PrintNoSlideError("justification", i); continue; }
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
                            continuingText = false;
                            break;
                        case "size":
                            if (CurrentSlide == null && string.IsNullOrEmpty(definingStyleName)) { PrintNoSlideError("size", i); continue; }
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
                            continuingText = false;
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
                            if (CurrentSlide == null && string.IsNullOrEmpty(definingStyleName)) { PrintNoSlideError("font", i); continue; }
                            if (!string.IsNullOrEmpty(t.Text)) t = new SlideText("");
                            if (!FontLibrary.Instance.HasFont(remainder))
                            {
                                Log($"Could not find font {remainder}. Was it declared?");
                                continue;
                            }
                            t.FontName = remainder;
                            continuingText = false;
                            break;
                        case "begin_style":
                            if (!string.IsNullOrEmpty(definingStyleName))
                            { 
                                Log($"Error on line {i}: attempt to define a new style, but we are already defining style {definingStyleName}");
                                continue;
                            }
                            if(styles.ContainsKey(remainder))
                            {
                                Log($"Attempt to redefine style {remainder}");
                                continue;
                            }
                            definingStyleName = remainder;
                            break;
                        case "end_style":
                            if (string.IsNullOrEmpty(definingStyleName))
                            {
                                Log($"Error on line {i}: attempt to end a style, but we are not defining one.");
                                continue;
                            }
                            styles[definingStyleName] = t;
                            t = new SlideText("");
                            definingStyleName = "";
                            break;
                        case "style":
                            if(!styles.ContainsKey(remainder))
                            {
                                Log($"Error on line {i}: undefined style {remainder}");
                                continue;
                            }
                            t = new SlideText(styles[remainder]);
                            break;
                        case "use_slide":
                            if(!templates.ContainsKey(remainder))
                            {
                                Log($"Error on line {i}: attempt to use undefined slide template {remainder}");
                                continue;
                            }
                            CurrentSlide?.CopyTemplate(templates[remainder]);
                            break;
                        case "right_margin":
                            if (CurrentSlide == null) { PrintNoSlideError("right margin", i); continue; };
                            try
                            {
                                CurrentSlide.RightMargin = float.Parse(remainder);
                            }
                            catch (FormatException e)
                            {
                                Log($"Error on line {i}: Expected float in right margin, but got {e.Message}");
                            }
                            break;
                        case "left_margin":
                            if (CurrentSlide == null) { PrintNoSlideError("left margin", i); continue; };
                            try
                            {
                                CurrentSlide.LeftMargin = float.Parse(remainder);
                            }
                            catch (FormatException e)
                            {
                                Log($"Error on line {i}: Expected float in left margin, but got {e.Message}");
                            }
                            break;
                        case "blank":
                            CurrentSlide?.CurrentSlideText?.PushText("\n");
                            break;
                        case "load_image":
                            var (alias, basename) = BreakBySpaces(remainder);
                            if(images.ContainsKey(alias))
                            {
                                Log($"Error on line {i}: attempt to redeclare image {alias}");
                            }
                            try
                            {
                                var imagePath = GetAnImageWithBasename(basename, Path.GetDirectoryName(path));
                                images[alias] = Image.FromFile(imagePath);
                            }
                            catch (FileNotFoundException)
                            {
                                Log($"Error on line {i}: Could not find any image with name {basename}");
                            }
                            break;
                        case "image":
                            if (CurrentSlide == null) { PrintNoSlideError("image", i); continue; };
                            try
                            {
                                SlideImage image = ParseImageArgs(remainder, i);
                                CurrentSlide.Add(image);
                            }
                            catch (FormatException) { continue; }
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
                        if(CurrentSlide.CurrentSlideText == null || !continuingText)
                        {
                            t.Text = line;
                            CurrentSlide.Add(t);
                            t = new SlideText(t) { Text = ""};
                            continuingText = true;
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

        private SlideImage ParseImageArgs(string remainder, int sourceLine)
        {
            string[] imgArgs = remainder.Split();
            if (!images.ContainsKey(imgArgs[0]))
            {
                Log($"Error on line {sourceLine}: There is no image with the name {imgArgs[0]} loaded.");
                throw new ArgumentException();
            }
            var image = new SlideImage(images[imgArgs[0]]);
            int i = 1;
            while (i < imgArgs.Length)
            {
                switch(imgArgs[i])
                {
                    case "pos":
                        if (!float.TryParse(imgArgs[i + 1], out image.x))
                        {
                            Log($"Error on line {sourceLine}: Could not parse x coordinate of pos");
                            throw new ArgumentException();
                        }
                        if (!float.TryParse(imgArgs[i + 2], out image.y))
                        {
                            Log($"Error on line {sourceLine}: Could not parse y coordinate of pos");
                            throw new ArgumentException();
                        }
                        i += 2;
                        break;
                    case "scale":
                        if (!float.TryParse(imgArgs[i + 1], out image.scale))
                        {
                            Log($"Error on line {sourceLine}: Could not parse scale");
                            throw new ArgumentException();
                        }
                        i++;
                        break;
                    default:
                        Log($"Error on line {sourceLine}: Unknown argument {imgArgs[i]}");
                        break;

                }
                i++;
            }
            
            return image;
        }

        private string GetAnImageWithBasename(string basename, string folder)
        {
            folder = Path.GetFullPath(folder);
            basename = Path.Combine(folder, basename);
            if (File.Exists(basename + ".jpg"))
                return basename + ".jpg";
             if (File.Exists(basename + ".png"))
                return basename + ".png";
            throw new FileNotFoundException();
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
