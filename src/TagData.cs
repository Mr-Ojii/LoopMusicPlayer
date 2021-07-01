using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMusicPlayer
{
    public class TagData
    {
        private Dictionary<string, IList<string>> _Tags;


        public IReadOnlyList<string> Performers => GetTagMulti("PERFORMER");

        public IReadOnlyList<string> Locations => GetTagMulti("LOCATION");

        public IReadOnlyList<string> Dates => GetTagMulti("DATE");

        public IReadOnlyList<string> Genres => GetTagMulti("GENRE");

        public string Description => GetTagSingle("DESCRIPTION");

        public string Organization => GetTagSingle("ORGANIZATION");

        public string License => GetTagSingle("LICENSE");

        public string Copyright => GetTagSingle("COPYRIGHT");

        public string Contact => GetTagSingle("CONTACT");

        public string Isrc => GetTagSingle("ISRC");

        public string TrackNumber => GetTagSingle("TRACKNUMBER");

        public string Album => GetTagSingle("ALBUM");

        public string Version => GetTagSingle("VERSION");

        public string Title => GetTagSingle("TITLE");

        public string EncoderVendor => throw new NotSupportedException();

        public IReadOnlyDictionary<string, IReadOnlyList<string>> All
        {
            get 
            {
                return (IReadOnlyDictionary<string, IReadOnlyList<string>>)_Tags;
            }
        }

        public string Artist => GetTagSingle("ARTIST");

        public TagData(string[] tags)
        {
            Dictionary<string, IList<string>> TmpTags = new Dictionary<string, IList<string>>();

            for (int i = 0; i < tags.Length; i++) 
            {
                string[] param = tags[i].Split('=');
                if (param.Length == 1)
                {
                    param = new string[] { param[0], string.Empty };
                }

                if (TmpTags.TryGetValue(param[0].ToUpperInvariant(), out var list))
                {
                    list.Add(param[1]);
                }
                else
                {
                    TmpTags.Add(param[0].ToUpperInvariant(), new List<string> { param[1] });
                }
            }
            this._Tags = TmpTags;
        }

        public string GetTagSingle(string key, bool concatenate = false)
        {
            var values = GetTagMulti(key);
            if (values.Count > 0)
            {
                if (concatenate)
                {
                    return string.Join('\n', values.ToArray());
                }
                return values[values.Count - 1];
            }
            return string.Empty;
        }

        public IReadOnlyList<string> GetTagMulti(string key) 
        {
            if (_Tags.TryGetValue(key.ToUpperInvariant(), out var values))
            {
                return (IReadOnlyList<string>)values;
            }
            return new List<string>();
        }
    }
}
