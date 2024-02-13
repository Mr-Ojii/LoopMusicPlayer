using System;
using Avalonia.Platform.Storage;

namespace LoopMusicPlayer.DataClass;

public class PlayListItem
{
    public string Title { get; private set; }
    public string Time { get; private set; }
    public string Artist { get; private set; }
    public string Loop { get; private set; }
    public string Path { get; private set; }
    public IStorageFile File { get; private set; }

    public PlayListItem(string title, string time, string artist, bool loop, string path, IStorageFile file)
    {
        this.Title = title;
        this.Time = time;
        this.Artist = artist;
        this.Loop = loop ? "Loop" : "" ;
        this.Path = path;
        this.File = file;
    }
}
