using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;
using System.Buffers;

namespace MemoryPack {

[Preserve]
public interface IMemoryPackFormatter
{
    [Preserve]
    void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref object? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>;
#else
        where TBufferWriter : class, IBufferWriter<byte>;
#endif
    [Preserve]
    void Deserialize(ref MemoryPackReader reader, ref object? value);
}

[Preserve]
public interface IMemoryPackFormatter<T>
{
    [Preserve]
    void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref T? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>;
#else
        where TBufferWriter : class, IBufferWriter<byte>;
#endif
    [Preserve]
    void Deserialize(ref MemoryPackReader reader, ref T? value);
}

[Preserve]
public abstract class MemoryPackFormatter<T> : IMemoryPackFormatter<T>, IMemoryPackFormatter
{
    [Preserve]
    public abstract void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref T? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>;
#else
        where TBufferWriter : class, IBufferWriter<byte>;
#endif
    [Preserve]
    public abstract void Deserialize(ref MemoryPackReader reader, ref T? value);

    [Preserve]
    void IMemoryPackFormatter.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref object? value)
    {
        var v = (T?)value;
        Serialize(ref writer, ref v);
    }

    [Preserve]
    void IMemoryPackFormatter.Deserialize(ref MemoryPackReader reader, ref object? value)
    {
        var v = (T?)value;
        Deserialize(ref reader, ref v);
        value = v;
    }
}

}