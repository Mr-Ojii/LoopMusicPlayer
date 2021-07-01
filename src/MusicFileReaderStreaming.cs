using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;

namespace LoopMusicPlayer
{
    internal class MusicFileReaderStreaming : IMusicFileReader
    {
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
        public TagData Tags
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
                return _SamplePosition;
            }
            set
            {
                if (value <= TotalSamples)
                {
                    this._SamplePosition = value;
                    Bass.ChannelSetPosition(this.handle, (long)(value * Const.float_per_byte * this.Channels), PositionFlags.Bytes);
                }
            }
        }
        private long _SamplePosition;
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
            set
            {
                double time = 0;
                time += value.Days * 86400;
                time += value.Hours * 3600;
                time += value.Minutes * 60;
                time += value.Seconds;
                time += value.Milliseconds * 0.001;
                this.SamplePosition = (long)(this.SampleRate * time);
            }
        }

        private int handle;

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

            if (Bass.LastError != Errors.OK)
                Console.WriteLine(Bass.LastError.ToString());

            string[] list = Extensions.ExtractMultiStringUtf8(Bass.ChannelGetTags(handle, TagType.OGG));
            this.Tags = new TagData(list);
        }

        public int ReadSamples(float[] buffer, int offset, int count)
        {
            if ((int)(this.TotalSamples - this.SamplePosition) * this.Channels < count)
                count = (int)(this.TotalSamples - this.SamplePosition) * this.Channels;

            Bass.ChannelGetData(this.handle, buffer, (int)(count * Const.float_per_byte));

            this._SamplePosition += (count / this.Channels);

            return count;
        }

        public void Dispose()
        {
            Bass.StreamFree(this.handle);
        }
    }
}
