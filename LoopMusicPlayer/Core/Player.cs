using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ManagedBass;
using LoopMusicPlayer.TagReaderExtensionMethods;

namespace LoopMusicPlayer.Core
{
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
        private static List<int> bassPluginsHandleList = new List<int>();

        [DllImport("libdl.so")]
        static extern IntPtr dlopen(string fileName, int flags);
    
        [DllImport("libdl.so")]
        static extern int dlclose(IntPtr libraryHandle);
        public static void Init(string BasePath) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                libraryHandle = dlopen(BasePath + "libbass.so", 0x101);
            }
            //Linux環境で、bassflacは2回以上Loadを実施しないと、正常にLoadできない(なんで?)
            for (int j = 0; j < 2; j++)
                for (int i = 0; i < bassPluginsList.Length; i++)
                {
                    string pluginName = bassPluginsList[i];
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        pluginName = pluginName + ".dll";
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        pluginName = "lib" + pluginName + ".dylib";
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        pluginName = "lib" + pluginName + ".so";
                    }
                    int pluginHandle = Bass.PluginLoad(BasePath + pluginName);
                    if (pluginHandle != 0)
                        bassPluginsHandleList.Add(pluginHandle);
                }
            Bass.Init();
            initialized = true;
        }

        public static void Free() {
            for (int i = 0; i < bassPluginsHandleList.Count; i++)
            {
                Bass.PluginFree(bassPluginsHandleList[i]);
            }
            bassPluginsHandleList.Clear();
            Bass.Free();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                dlclose(libraryHandle);
            libraryHandle = IntPtr.Zero;
            initialized = false;
        }
        private static bool initialized = false;
        private static IntPtr libraryHandle = IntPtr.Zero;

        private IMusicFileReader reader = null;
        private int StreamHandle = -1;
        private StreamProcedure tSTREAMPROC = null;
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
                lock(LockObj)
                    return _LoopCount;
            }
            private set
            {
                lock(LockObj)
                    _LoopCount = value;
            }
        }

        public TimeSpan LoopStartTime
        {
            get
            {
                double time = (LoopStart / (double)reader.SampleRate);
                int millisecond = (int)((time % 1) * 1000);
                int second = (int)(time % 60);
                int minute = (int)(time / 60) % 60;
                int hour = (int)(time / 3600) % 24;
                int day = (int)(time / 86400);
                return new TimeSpan(day, hour, minute, second, millisecond);
            }
        }

        public TimeSpan LoopEndTime
        {
            get
            {
                double time = (LoopEnd / (double)reader.SampleRate);
                int millisecond = (int)((time % 1) * 1000);
                int second = (int)(time % 60);
                int minute = (int)(time / 60) % 60;
                int hour = (int)(time / 3600) % 24;
                int day = (int)(time / 86400);
                return new TimeSpan(day, hour, minute, second, millisecond);
            }
        }


        public long TotalSamples
        {
            get
            {
                return reader.TotalSamples;
            }
        }

        public long SamplePosition
        {
            get
            {
                return reader.SamplePosition;
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                return reader.TotalTime;
            }
        }

        public TimeSpan TimePosition
        {
            get
            {
                return reader.TimePosition;
            }
        }

        public string Title
        {
            get
            {
                return !string.IsNullOrEmpty(reader.Tags.Title) ? reader.Tags.Title : System.IO.Path.GetFileName(FilePath);
            }
        }

        public string Artist
        {
            get
            {
                return !string.IsNullOrEmpty(reader.Tags.Artist) ? reader.Tags.Artist : string.Empty;
            }
        }

        public TagReader Tags
        {
            get 
            {
                return reader.Tags;
            }
        }

        public event EventHandler LoopAction;

        public event EventHandler EndAction;

        public readonly string FilePath;

        private bool Ended;

        public Player(string filepath, double volume)
        {
            if(!initialized)
                throw new Exception("Player class is not initialized.");
            this.FilePath = filepath;
            Ended = false;
            this.reader = new MusicFileReaderStreaming(filepath);

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

        public void ChangeVolume(double volume)//0~1
        {
            Bass.ChannelSetAttribute(this.StreamHandle, ChannelAttribute.Volume, volume);
        }

        public void Play()
        {
            Bass.ChannelPlay(this.StreamHandle);
        }

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
        {
            return Bass.ChannelIsActive(this.StreamHandle);
        }

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
                int tmplength = (int)((LoopEnd - reader.SamplePosition)* reader.Channels * Const.float_per_byte);
                num += reader.ReadSamples(buffer, 0, tmplength);
                reader.SamplePosition = LoopStart;
                num += reader.ReadSamples(buffer, tmplength, length - tmplength);
                this.LoopCount++;
                if (LoopAction != null) LoopAction(this, EventArgs.Empty);
            }
            else
            {
                num = reader.ReadSamples(buffer, 0, length);
            }

            if (num < 0) num = 0;

            if(this.EndAction != null && num != length) {
                if(!Ended) {
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
}
