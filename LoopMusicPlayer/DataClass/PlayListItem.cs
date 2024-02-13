using System;
using Avalonia.Platform.Storage;

namespace LoopMusicPlayer.DataClass;

public class PlayListItem
{
    public string Title { get; set; }
    public string Time { get; set; }
    public string Artist { get; set; }
    public string Loop { get; set; }
    public string Path { get; set; }
    public Uri Uri { get; set; }

    public PlayListItem(string title, string time, string artist, bool loop, string path, Uri uri)
    {
        this.Title = title;
        this.Time = time;
        this.Artist = artist;
        this.Loop = loop ? "Loop" : "" ;
        this.Path = path;
        this.Uri = uri;
    }
}
