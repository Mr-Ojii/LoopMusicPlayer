using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;

namespace LoopMusicPlayer.Core
{
    internal class MusicFileReader : IMusicFileReader
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
                return _SamplePosition;
            }
            set
            {
                if (value <= TotalSamples)
                    this._SamplePosition = value;
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

        private float[] Buf;

        public MusicFileReader(string FilePath)
        {
            int handle = Bass.SampleLoad(FilePath, 0, 0, 1, BassFlags.Float);

            if (Bass.LastError != Errors.OK)
                throw new Exception(Bass.LastError.ToString());

            this.Buf = new float[(long)(Bass.ChannelGetLength(handle, PositionFlags.Bytes) * Const.byte_per_float)];

            Bass.SampleGetData(handle, this.Buf);

            if (Bass.LastError != Errors.OK)
                Console.WriteLine(Bass.LastError.ToString());

            int channel = Bass.SampleGetChannel(handle);
            ChannelInfo info = Bass.ChannelGetInfo(channel);

            this.SampleRate = info.Frequency;
            this.Channels = info.Channels;
            this.TotalSamples = Buf.Length / this.Channels;
            this.SamplePosition = 0;

            int tmphandle = Bass.CreateStream(FilePath);

            this.Tags = TagReader.Read(tmphandle);

            Bass.StreamFree(tmphandle);

            Bass.SampleFree(handle);
        }

        public int ReadSamples(float[] buffer, int offset, int count)
        {
            if ((int)(this.TotalSamples - this.SamplePosition) * this.Channels < count)
                count = (int)(this.TotalSamples - this.SamplePosition) * this.Channels;

            if(count != 0)
                Buffer.BlockCopy(this.Buf, (int)(this.SamplePosition * this.Channels * (Const.float_per_byte)), buffer, (int)(offset * Const.float_per_byte), (int)(count * Const.float_per_byte));

            this.SamplePosition += (count / this.Channels);

            return count;
        }

        public void Dispose() 
        {
        }
    }
}
