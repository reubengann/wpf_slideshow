using System;
using System.Collections.Generic;
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

        public void Add(string key, Uri path, string name)
        {
            if(_fontFileToNameMap.ContainsKey(path.LocalPath))
            {
                _fonts[key] = _fonts[_fontFileToNameMap[path.LocalPath]];
                return;
            }
            FontFamily f = new FontFamily(path+"#"+name);
            _fonts[key] = f;
        }

        public FontFamily Get(string key)
        {
            return _fonts[key];
        }
    }
}
