using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;

namespace LoopMusicPlayer.Core
{
    internal class MusicFileReaderStreaming : IMusicFileReader
    {
        private object LockObj = new object();
        public long TotalSamples
        {
            get;
        }
        public TimeSpan TotalTime
        {
            get
            {
                double time = (this.TotalSamples / (double)this.SampleRate);
                int millisecond = (int)((time % 1) * 1000);
                int second = (int)(time % 60);
                int minute = (int)(time / 60) % 60;
                int hour = (int)(time / 3600) % 24;
                int day = (int)(time / 86400);
                return new TimeSpan(day, hour, minute, second, millisecond);
            }
        }
        public TagReader Tags
        {
            get;
        }
        public int SampleRate
        {
            get;
        }
        public int Channels
        {
            get;
        }
        public long SamplePosition
        {
            get
            {
                lock (LockObj)
                {
                    return (long)(Bass.ChannelGetPosition(this.handle, PositionFlags.Bytes) * Const.byte_per_float / this.Channels);
                }
            }
            set
            {
                lock (LockObj)
                {
                    if (value <= TotalSamples)
                    {
                        Bass.ChannelSetPosition(this.handle, (long)(value * Const.float_per_byte * this.Channels), PositionFlags.Bytes);
                    }
                }
            }
        }
        public TimeSpan TimePosition
        {
            get
            {
                double time = (this.SamplePosition / (double)this.SampleRate);
                int millisecond = (int)((time % 1) * 1000);
                int second = (int)(time % 60);
                int minute = (int)(time / 60) % 60;
                int hour = (int)(time / 3600) % 24;
                int day = (int)(time / 86400);
                return new TimeSpan(day, hour, minute, second, millisecond);
            }
        }

        private int handle = 0;

        public MusicFileReaderStreaming(string FilePath)
        {
            this.handle = Bass.CreateStream(FilePath, Flags: BassFlags.Float | BassFlags.Decode);

            if (Bass.LastError != Errors.OK)
                throw new Exception(Bass.LastError.ToString());

            ChannelInfo info = Bass.ChannelGetInfo(handle);

            this.SampleRate = info.Frequency;
            this.Channels = info.Channels;
            this.TotalSamples = (long)(Bass.ChannelGetLength(handle, PositionFlags.Bytes) * Const.byte_per_float) / this.Channels;
            this.SamplePosition = 0;

            this.Tags = TagReader.Read(this.handle);
        }

        public int ReadSamples(IntPtr buffer, int byte_offset, int byte_count)
        {
            return Bass.ChannelGetData(this.handle, IntPtr.Add(buffer, byte_offset), byte_count);
        }

        public void Dispose()
        {
            Bass.StreamFree(this.handle);
        }
    }
}
