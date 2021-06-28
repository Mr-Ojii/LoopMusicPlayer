using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVorbis;
using NVorbis.Contracts;

namespace LoopMusicPlayer
{
    internal class MusicFileReader : IVorbisReader
    {
        public int UpperBitrate
        {
            get;
        }
        public long TotalSamples
        {
            get;
        }
        public TimeSpan TotalTime
        {
            get;
        }
        public int LowerBitrate
        {
            get;
        }
        public int NominalBitrate
        {
            get;
        }
        public ITagData Tags
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
        public int StreamIndex
        {
            get;
        }
        public IReadOnlyList<IStreamDecoder> Streams
        {
            get;
        }
        public long ContainerWasteBits 
        {
            get;
        }
        public long ContainerOverheadBits
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
        public bool HasClipped 
        {
            get;
        }
        public bool IsEndOfStream 
        {
            get 
            {
                return this.TotalSamples == this.SamplePosition;
            }
        }
        public IStreamStats StreamStats 
        {
            get;
        }
        public bool ClipSamples
        {
            get;
            set;
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

        public event EventHandler<NewStreamEventArgs> NewStream;
        private float[] Buf;

        public MusicFileReader(string FilePath)
        {
            using (VorbisReader reader = new VorbisReader(FilePath))
            {
                this.Tags = reader.Tags;
                this.HasClipped = reader.HasClipped;
                this.ClipSamples = reader.ClipSamples;

                this.TotalSamples = reader.TotalSamples;
                this.SampleRate = reader.SampleRate;
                this.TotalTime = reader.TotalTime;
                this.Channels = reader.Channels;
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

        public bool FindNextStream() => throw new NotSupportedException();

        public int ReadSamples(float[] buffer, int offset, int count)
        {
            if (this.TotalSamples - this.SamplePosition < count) 
                count = (int)(this.TotalSamples - this.SamplePosition);

            Buffer.BlockCopy(this.Buf, (int)this.SamplePosition * this.Channels * (sizeof(float) / sizeof(byte)), buffer, offset * (sizeof(float) / sizeof(byte)), count * (sizeof(float) / sizeof(byte)));

            this.SamplePosition += (count / this.Channels);

            return count;
        }

        public void SeekTo(TimeSpan timePosition, SeekOrigin seekOrigin = SeekOrigin.Begin) 
        {
            this.TimePosition = timePosition;
        }

        public void SeekTo(long samplePosition, SeekOrigin seekOrigin = SeekOrigin.Begin)
        {
            this.SamplePosition = samplePosition;
        }

        public bool SwitchStreams(int index) => throw new NotSupportedException();

        public void Dispose() 
        {
        }
    }
}
