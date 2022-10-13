using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Buffers;

namespace MemoryPack {

public interface IMemoryPackFormatter
{
    void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref object? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>;
#else
        where TBufferWriter : class, IBufferWriter<byte>;
#endif
    void Deserialize(ref MemoryPackReader reader, ref object? value);
}

public interface IMemoryPackFormatter<T>
{
    void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref T? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>;
#else
        where TBufferWriter : class, IBufferWriter<byte>;
#endif        
    void Deserialize(ref MemoryPackReader reader, ref T? value);
}

public abstract class MemoryPackFormatter<T> : IMemoryPackFormatter<T>, IMemoryPackFormatter
{
    public abstract void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref T? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>;
#else
        where TBufferWriter : class, IBufferWriter<byte>;
#endif
    public abstract void Deserialize(ref MemoryPackReader reader, ref T? value);

    void IMemoryPackFormatter.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref object? value)
    {
        var v = (T?)value;
        Serialize(ref writer, ref v);
    }

    void IMemoryPackFormatter.Deserialize(ref MemoryPackReader reader, ref object? value)
    {
        var v = (T?)value;
        Deserialize(ref reader, ref v);
        value = v;
    }
}

}