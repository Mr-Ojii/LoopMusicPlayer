﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ManagedBass;
using LoopMusicPlayer.TagReaderExtensionMethods;
using System.IO;
using BassLibraryLoader;


namespace LoopMusicPlayer.Core;

public class Player : IDisposable
{
    private static string[] bassPluginsList = {
        "bass_aac",
        "bass_ac3",
        "bass_adx",
        "bass_aix",
        "bassalac",
        "bass_ape",
        "bassdsd",
        "bassflac",
        "bass_mpc",
        "bass_ofr",
        "bassopus",
        "bass_spx",
        "bass_tta",
        "basswebm",
        "basswma",
        "basswv",
        };
    private static List<int> bassPluginsHandleList { get; set; } = new();
    public static List<Tuple<string, string>> bassPluginsVersionList { get; private set; } = new();
    private static CBassLibraryLoader? bassLibraryLoader = null;
    public static void Init(string BasePath, int Device = -1)
    {
        bassLibraryLoader = new CBassLibraryLoader();

        //Linux環境で、bassflacは2回以上Loadを実施しないと、正常にLoadできない(なんで?)
        for (int j = 0; j < 2; j++)
            for (int i = 0; i < bassPluginsList.Length; i++)
            {
                string pluginName = bassPluginsList[i];
                if (OperatingSystem.IsWindows())
                {
                    pluginName = BasePath + $"{pluginName}.dll";
                }
                else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
                {
                    pluginName = BasePath + $"lib{pluginName}.dylib";
                }
                else if (OperatingSystem.IsLinux())
                {
                    pluginName = BasePath + $"lib{pluginName}.so";
                }
                else if (OperatingSystem.IsAndroid())
                {
                    pluginName = $"lib{pluginName}.so";
                }
                int pluginHandle = Bass.PluginLoad(pluginName);
                if (pluginHandle != 0) {
                    PluginInfo info = Bass.PluginGetInfo(pluginHandle);
                    bassPluginsHandleList.Add(pluginHandle);
                    bassPluginsVersionList.Add(new Tuple<string, string>(bassPluginsList[i].Replace("_", "").ToUpperInvariant(), info.Version.ToString()));
                }
            }
        Bass.Init(Device);
        initialized = true;
    }

    public static void Free()
    {
        foreach (var i in bassPluginsHandleList)
            Bass.PluginFree(i);
        bassPluginsHandleList.Clear();
        bassPluginsVersionList.Clear();
        Bass.Free();
        bassLibraryLoader?.Dispose();
        bassLibraryLoader = null;
        initialized = false;
    }
    private static bool initialized = false;

    private IMusicFileReader reader;
    private int StreamHandle = -1;
    private StreamProcedure tSTREAMPROC;
    private uint _LoopCount = 0;
    private object LockObj = new object();

    public readonly bool IsLoop = false;
    public readonly long LoopStart = 0;
    public readonly long LoopEnd = 0;
    public bool NextIsLoop = true;
    public uint LoopCount
    {
        get
        {
            lock (LockObj)
                return _LoopCount;
        }
        private set
        {
            lock (LockObj)
                _LoopCount = value;
        }
    }

    public TimeSpan LoopStartTime => TimeSpan.FromSeconds(LoopStart / (double)reader.SampleRate);

    public TimeSpan LoopEndTime => TimeSpan.FromSeconds(LoopEnd / (double)reader.SampleRate);

    public long TotalSamples => reader.TotalSamples;

    public long SamplePosition => reader.SamplePosition;

    public int SampleRate => reader.SampleRate;

    public TimeSpan TotalTime => reader.TotalTime;

    public TimeSpan TimePosition => reader.TimePosition;

    public string Title => !string.IsNullOrEmpty(reader.Tags.Title) ? reader.Tags.Title : ("/" + System.IO.Path.GetFileName(FilePath));

    public string Artist => !string.IsNullOrEmpty(reader.Tags.Artist) ? reader.Tags.Artist : string.Empty;

    public event EventHandler? LoopAction = null;

    public event EventHandler? EndAction = null;

    public readonly string FilePath;

    private bool Ended;

    public Player(string filepath, double volume, bool streaming, Stream? stream = null)
    {
        if (!initialized)
            throw new Exception("Player class is not initialized.");
        this.FilePath = filepath;
        Ended = false;

        if (streaming)
        {
            if (stream is not null)
                this.reader = new MusicFileReaderStreaming(stream);
            else
                this.reader = new MusicFileReaderStreaming(filepath);
        }
        else
        {
            if (stream is not null)
                this.reader = new MusicFileReaderMemory(stream);
            else
                this.reader = new MusicFileReaderMemory(filepath);
        }

        this.IsLoop = !string.IsNullOrEmpty(reader.Tags.GetTag("LOOPSTART")) && (!string.IsNullOrEmpty(reader.Tags.GetTag("LOOPLENGTH")) || !string.IsNullOrEmpty(reader.Tags.GetTag("LOOPEND")));
        if (this.IsLoop)
        {
            LoopStart = long.Parse(reader.Tags.GetTag("LOOPSTART"));
            if (!string.IsNullOrEmpty(reader.Tags.GetTag("LOOPLENGTH")))
            {
                LoopEnd = LoopStart + long.Parse(reader.Tags.GetTag("LOOPLENGTH"));
            }
            else
            {
                LoopEnd = long.Parse(reader.Tags.GetTag("LOOPEND"));
            }
        }
        else
        {
            LoopStart = 0;
            LoopEnd = reader.TotalSamples;
        }

        this.tSTREAMPROC = new StreamProcedure(this.StreamProc);
        this.StreamHandle = Bass.CreateStream(reader.SampleRate, reader.Channels, BassFlags.Float, this.tSTREAMPROC);
        ChangeVolume(volume);
    }

    public void Seek(long sample)
    {
        this.reader.SamplePosition = sample;

        if (sample >= this.LoopEnd)
            this.NextIsLoop = false;
    }

    public void ChangeVolume(double volume)
        => Bass.ChannelSetAttribute(this.StreamHandle, ChannelAttribute.Volume, Math.Clamp(volume, 0, 1));

    public void Play()
        => Bass.ChannelPlay(this.StreamHandle);

    public void Pause()
    {
        switch (Bass.ChannelIsActive(this.StreamHandle))
        {
            case PlaybackState.Paused:
                Bass.ChannelPlay(this.StreamHandle);
                break;
            case PlaybackState.Playing:
                Bass.ChannelPause(this.StreamHandle);
                break;
            default:
                break;
        }
    }

    public void Stop()
    {
        Bass.ChannelStop(this.StreamHandle);
        this.reader.SamplePosition = 0;
    }

    public PlaybackState Status()
        => Bass.ChannelIsActive(this.StreamHandle);

    public bool CheckDeviceEnable()
    {
        if (Bass.GetDeviceInfo(Bass.CurrentDevice, out DeviceInfo info))
        {
            return info.IsEnabled;
        }
        return false;
    }

    public int StreamProc(int handle, IntPtr buffer, int length, IntPtr user)
    {
        int num = 0;

        if (NextIsLoop && reader.SamplePosition + (int)(Const.byte_per_float * (length / reader.Channels)) >= LoopEnd)
        {
            int tmplength = (int)((LoopEnd - reader.SamplePosition) * reader.Channels * Const.float_per_byte);
            num += reader.ReadSamples(buffer, 0, tmplength);
            reader.SamplePosition = LoopStart;
            num += reader.ReadSamples(buffer, tmplength, length - tmplength);
            this.LoopCount++;
            if (LoopAction is not null) LoopAction(this, EventArgs.Empty);
        }
        else
        {
            num = reader.ReadSamples(buffer, 0, length);
        }

        if (num < 0) num = 0;

        if (this.EndAction is not null && num != length)
        {
            if (!Ended)
            {
                Ended = true;
                this.EndAction(this, EventArgs.Empty);
            }
        }
        else
            Ended = false;

        return num;
    }

    public void Dispose()
    {
        try
        {
            if (this.StreamHandle != -1)
            {
                Bass.ChannelStop(this.StreamHandle);
                Bass.StreamFree(this.StreamHandle);
            }
            reader?.Dispose();
        }
        catch (Exception e)
        {
            Trace.TraceError(e.ToString());
        }
    }
}
