using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;

namespace LoopMusicPlayer.TagReaderExtensionMethods
{
    public static class TagReaderExtensions
    {
        public static string GetTag(this TagReader tagReader, string key)
        {
            if (tagReader.Other.TryGetValue(key, out string value)) 
            {
                return value;
            }
            return string.Empty;
        }
    }
}
