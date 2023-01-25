using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;

namespace LoopMusicPlayer.Core
{
    public interface IMusicFileReader : IDisposable 
    {
        long TotalSamples { get; }
        TimeSpan TotalTime { get; }
        TagReader Tags { get; }
        int SampleRate { get; }
        int Channels { get; }
        long SamplePosition { get; set; }
        TimeSpan TimePosition { get; }

        int ReadSamples(IntPtr buffer, int byte_offset, int byte_count);
    }
}
