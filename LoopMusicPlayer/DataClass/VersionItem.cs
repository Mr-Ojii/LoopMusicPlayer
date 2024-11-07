using System;

namespace LoopMusicPlayer.DataClass;

public class VersionItem
{
    public string Name { get; }
    public string Version { get; }

    public VersionItem(string name, string version)
    {
        this.Name = name;
        this.Version = version;
    }
}
