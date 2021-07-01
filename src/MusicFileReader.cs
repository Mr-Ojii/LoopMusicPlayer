using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVorbis.Contracts;
using ManagedBass;

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
        public bool HasClipped => throw new NotSupportedException();
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

        public event EventHandler<NVorbis.NewStreamEventArgs> NewStream;
        private float[] Buf;

        public MusicFileReader(string FilePath)
        {
            int handle = Bass.SampleLoad(FilePath, 0, 0, 1, BassFlags.Float);
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
            this.ClipSamples = false;

            if (Bass.LastError != Errors.OK)
                Console.WriteLine(Bass.LastError.ToString());

            int tmphandle = Bass.CreateStream(FilePath);

            string[] list = Extensions.ExtractMultiStringUtf8(Bass.ChannelGetTags(tmphandle, TagType.OGG));
            this.Tags = new TagData(list);

            Bass.StreamFree(tmphandle);

            Bass.SampleFree(handle);
        }

        public bool FindNextStream() => throw new NotSupportedException();

        public int ReadSamples(float[] buffer, int offset, int count)
        {
            if (this.TotalSamples - this.SamplePosition < count) 
                count = (int)(this.TotalSamples - this.SamplePosition);

            Buffer.BlockCopy(this.Buf, (int)(this.SamplePosition * this.Channels * (Const.float_per_byte)), buffer, (int)(offset * Const.float_per_byte), (int)(count * Const.float_per_byte));

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
