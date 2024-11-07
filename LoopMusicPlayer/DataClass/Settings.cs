using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoopMusicPlayer.DataClass;

public class Settings
{
    public static string _settingPath = Path.Combine(AppContext.BaseDirectory, "LoopMusicPlayerSetting.json");

    public static Settings Load()
    {
        if (File.Exists(_settingPath))
        {
            try
            {
                Settings? settings = JsonSerializer.Deserialize<Settings>(File.ReadAllBytes(_settingPath), SettingsSourceGenerationContext.Default.Settings);
                if (settings is not null)
                    return settings;
            }
            catch(Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }
        return new Settings();
    }

    public void Save()
    {
        try
        {
            File.WriteAllBytes(_settingPath, JsonSerializer.SerializeToUtf8Bytes(this, SettingsSourceGenerationContext.Default.Settings));
        }
        catch (Exception e)
        {
            Trace.TraceWarning(e.ToString());
        }
    }

    public CGeneral General { get; set; } = new();
    public class CGeneral
    {
        public bool EnableLoop { get; set; } = true;
        public ERepeatType RepeatType
        {
            get => this._repeatType;
            set => this._repeatType = (ERepeatType)Math.Clamp((int)value, 0, (int)ERepeatType.Max - 1);
        }
        private ERepeatType _repeatType = ERepeatType.All;
        public enum ERepeatType
        {
            Disable = 0,
            Single,
            All,
            Random,
            Max,
        }
    }
    public CAudio Audio { get; set; } = new();
    public class CAudio
    {
        public int UpdatePeriod
        {
            get => this._updatePeriod;
            set => this._updatePeriod = Math.Clamp(value, 5, 100);
        }
        private int _updatePeriod = 50;

        public int BufferLength
        {
            get => this._bufferLength;
            set => this._bufferLength = Math.Clamp(value, _updatePeriod, 500);
        }
        private int _bufferLength = 100;
        public EPlaybackType PlaybackType
        {
            get => this._playbackType;
            set => this._playbackType = (EPlaybackType)Math.Clamp((int)value, 0, (int)EPlaybackType.Max - 1);
        }
        private EPlaybackType _playbackType = EPlaybackType.Streaming;
        public enum EPlaybackType
        {
            Streaming,
            OnMemory,
            Max,
        }
        public double Volume
        {
            get => this._volume;
            set => this._volume = Math.Clamp(value, 0, 1);
        }
        private double _volume = 1.0;
    }
    public CView View { get; set; } = new();
    public class CView
    {
        public ETimeFormat TimeFormat
        {
            get => this._timeForrmat;
            set => this._timeForrmat = (ETimeFormat)Math.Clamp((int)value, 0, (int)ETimeFormat.Max - 1);
        }
        private ETimeFormat _timeForrmat = ETimeFormat.SeekTime;

        public bool TopMost { get; set ;} = false;

        public enum ETimeFormat
        {
            ElapsedTime = 0,
            SeekTime,
            RemainingTime,
            Max,
        }
    }
}

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(Settings))]
internal partial class SettingsSourceGenerationContext : JsonSerializerContext
{
}
