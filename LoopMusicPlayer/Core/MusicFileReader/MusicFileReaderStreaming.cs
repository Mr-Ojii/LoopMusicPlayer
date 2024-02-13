using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using System.Buffers;

namespace LoopMusicPlayer.Core;

internal class MusicFileReaderStreaming : IMusicFileReader
{
    private object LockObj = new object();
    public long TotalSamples
    {
        get;
    }
    public TimeSpan TimePosition => TimeSpan.FromSeconds(this.SamplePosition / (double)this.SampleRate);
    public TimeSpan TotalTime => TimeSpan.FromSeconds(this.TotalSamples / (double)this.SampleRate);
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

    private int handle = 0;

    public MusicFileReaderStreaming(Stream stream)
    {
        byte[] array = ArrayPool<byte>.Shared.Rent((int)stream.Length);
        stream.Read(array, 0, (int)stream.Length);
        this.handle = Bass.CreateStream(array, 0, stream.Length, Flags: BassFlags.Float | BassFlags.Decode);
        ArrayPool<byte>.Shared.Return(array);

        if (Bass.LastError != Errors.OK)
            throw new Exception(Bass.LastError.ToString());

        ChannelInfo info = Bass.ChannelGetInfo(handle);

        this.SampleRate = info.Frequency;
        this.Channels = info.Channels;
        this.TotalSamples = (long)(Bass.ChannelGetLength(handle, PositionFlags.Bytes) * Const.byte_per_float) / this.Channels;
        this.SamplePosition = 0;

        this.Tags = TagReader.Read(this.handle);
    }

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
