using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;

namespace LoopMusicPlayer;

internal class LanguageManager
{
    public List<LanguageData> languageDatas
    {
        get;
        private set;
    } = new();
    internal LanguageManager()
    {
        try
        {
            DirectoryInfo info = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, @"lang"));

            foreach (FileInfo fileinfo in info.GetFiles())
            {
                if (fileinfo.Extension.ToLower() != ".json")
                    continue;
                try
                {
                    LanguageData data = JsonSerializer.Deserialize<LanguageData>(fileinfo.Open(FileMode.Open));
                    if (!string.IsNullOrEmpty(data.LanguageName))
                    {
                        languageDatas.Add(data);
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                }
            }
            languageDatas.Sort((a, b) => a.LanguageName.CompareTo(b.LanguageName));
        }
        catch (Exception e)
        {
            Trace.TraceError(e.ToString());
        }
    }
}
