using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Show
{
    public class FontLibrary
    {
        Dictionary<string, FontFamily> _fonts;
        Dictionary<string, string> _fontFileToNameMap;
        private static readonly Lazy<FontLibrary> lazy =
            new Lazy<FontLibrary>(() => new FontLibrary());

        public static FontLibrary Instance { get { return lazy.Value; } }

        private FontLibrary()
        {
            _fonts = new Dictionary<string, FontFamily>();
            _fontFileToNameMap = new Dictionary<string, string>();
        }

        public void Add(string key, Uri path)
        {
            if(_fontFileToNameMap.ContainsKey(path.LocalPath))
            {
                _fonts[key] = _fonts[_fontFileToNameMap[path.LocalPath]];
                return;
            }
            FontFamily f = LoadFontFromFile(path.LocalPath);
            _fonts[key] = f;
        }

        public FontFamily Get(string key)
        {
            return _fonts[key];
        }

        public static FontFamily LoadFontFromFile(string Filename)
        {
            PrivateFontCollection fontCol = new PrivateFontCollection();
            fontCol.AddFontFile(Filename);
            FontFamily fontFamily = new FontFamily(Filename + "#" + fontCol.Families.First().Name);
            return fontFamily;

        }

        public bool HasFont(string key)
        {
            return _fonts.ContainsKey(key);
        }
    }
}
