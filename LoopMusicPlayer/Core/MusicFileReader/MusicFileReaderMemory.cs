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

internal class MusicFileReaderMemory : IMusicFileReader
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
                return (long)(Bass.ChannelGetPosition(this.stream, PositionFlags.Bytes) * Const.byte_per_float / this.Channels);
            }
        }
        set
        {
            lock (LockObj)
            {
                if (value <= TotalSamples)
                {
                    Bass.ChannelSetPosition(this.stream, (long)(value * Const.float_per_byte * this.Channels), PositionFlags.Bytes);
                }
            }
        }
    }

    private int handle = 0;
    private int stream = 0;

    public MusicFileReaderMemory(Stream stream)
    {
        byte[] array = ArrayPool<byte>.Shared.Rent((int)stream.Length);
        stream.Read(array, 0, (int)stream.Length);
        this.handle = Bass.SampleLoad(array, 0, (int)stream.Length, 1, Flags: BassFlags.Float);
        ArrayPool<byte>.Shared.Return(array);

        if (this.handle == 0 && Bass.LastError != Errors.OK)
            throw new Exception(Bass.LastError.ToString());

        this.stream = Bass.SampleGetChannel(this.handle, BassFlags.Decode | BassFlags.SampleChannelStream);
        ChannelInfo info = Bass.ChannelGetInfo(this.stream);

        this.SampleRate = info.Frequency;
        this.Channels = info.Channels;
        this.TotalSamples = (long)(Bass.ChannelGetLength(this.stream, PositionFlags.Bytes) * Const.byte_per_float) / this.Channels;
        this.SamplePosition = 0;

        int tmphandle = Bass.CreateStream(array, 0, stream.Length, Flags: BassFlags.Float | BassFlags.Decode);
        this.Tags = TagReader.Read(tmphandle);
        Bass.StreamFree(tmphandle);
    }

    public MusicFileReaderMemory(string FilePath)
    {
        this.handle = Bass.SampleLoad(FilePath, 0, 0, 1, Flags: BassFlags.Float);

        if (Bass.LastError != Errors.OK)
            throw new Exception(Bass.LastError.ToString());

        this.stream = Bass.SampleGetChannel(this.handle, BassFlags.Decode | BassFlags.SampleChannelStream);
        ChannelInfo info = Bass.ChannelGetInfo(this.stream);

        this.SampleRate = info.Frequency;
        this.Channels = info.Channels;
        this.TotalSamples = (long)(Bass.ChannelGetLength(this.stream, PositionFlags.Bytes) * Const.byte_per_float) / this.Channels;
        this.SamplePosition = 0;

        int tmphandle = Bass.CreateStream(FilePath);
        this.Tags = TagReader.Read(tmphandle);
        Bass.StreamFree(tmphandle);
    }

    public int ReadSamples(IntPtr buffer, int byte_offset, int byte_count)
    {
        return Bass.ChannelGetData(this.stream, IntPtr.Add(buffer, byte_offset), byte_count);
    }

    public void Dispose()
    {
        Bass.SampleFree(this.handle);
        this.handle = 0;
    }
}
