using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;

namespace LoopMusicPlayer
{
    public interface IMusicFileReader : IDisposable 
    {
        public long TotalSamples { get; }
        public TimeSpan TotalTime { get; }
        public TagReader Tags { get; }
        public int SampleRate { get; }
        public int Channels { get; }
        public long SamplePosition { get; set; }
        public TimeSpan TimePosition { get; set; }

        public int ReadSamples(float[] buffer, int offset, int count);
    }
}
