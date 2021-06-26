using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVorbis;
using NVorbis.Contracts;

namespace LoopMusicPlayer
{
    internal class MusicFileReader
    {
        private float[] Buf;
        public readonly long TotalSamples;
        public readonly TimeSpan TotalTime;
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
        }
        public readonly int SampleRate;
        public readonly int Channels;
        public readonly ITagData Tags;

        public MusicFileReader(string FilePath)
        {
            using (VorbisReader reader = new VorbisReader(FilePath))
            {
                this.TotalSamples = reader.TotalSamples;
                this.SampleRate = reader.SampleRate;
                this.TotalTime = reader.TotalTime;
                this.Channels = reader.Channels;
                this.Tags = reader.Tags;
                this.SamplePosition = 0;
                this.Buf = new float[this.TotalSamples * this.Channels];

                var readBuffer = new float[reader.Channels * reader.SampleRate * 5];


                int cnt;
                int p = 0;

                while ((cnt = reader.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    if (this.Buf.Length < p + cnt)
                    {
                        cnt = this.Buf.Length - p;
                    }

                    Buffer.BlockCopy(readBuffer, 0, this.Buf, p * 4, cnt * 4);
                    p += cnt;
                }
            }
        }

        public int ReadSamples(ref float[] buffer, int offset, int count)
        {
            if (this.TotalSamples - this.SamplePosition < count) 
                count = (int)(this.TotalSamples - this.SamplePosition);

            Buffer.BlockCopy(this.Buf, (int)this.SamplePosition * this.Channels * (sizeof(float) / sizeof(byte)), buffer, offset * (sizeof(float) / sizeof(byte)), count * (sizeof(float) / sizeof(byte)));

            this.SamplePosition += (count / this.Channels);

            return count;
        }
        public void Dispose() 
        {
        }
    }
}
