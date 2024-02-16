using System;

namespace LoopMusicPlayer.DataClass;

public class LicenseItem
{
    public string Name { get; }
    public string Text { get; }

    public LicenseItem(string name, string text)
    {
        this.Name = name;
        this.Text = text;
    }
}
