﻿using System.Buffers;

namespace MemoryPack;

public interface IMemoryPackFormatter
{
    void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref object? value)
        where TBufferWriter : IBufferWriter<byte>;
    void Deserialize(ref MemoryPackReader reader, scoped ref object? value);

    void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref object? value);
}

public interface IMemoryPackFormatter<T>
{
    void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    void Deserialize(ref MemoryPackReader reader, scoped ref T? value);

    void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref T? value);
}

public abstract class MemoryPackFormatter<T> : IMemoryPackFormatter<T>, IMemoryPackFormatter
{
    public abstract void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    public abstract void Deserialize(ref MemoryPackReader reader, scoped ref T? value);

    public abstract void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref T? value);

    void IMemoryPackFormatter.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref object? value)
    {
        var v = (T?)value;
        Serialize(ref writer, ref v);
    }

    void IMemoryPackFormatter.Serialize(ref DoNothingMemoryPackWriter writer, scoped ref object? value)
    {
        var v = (T?)value;
        Serialize(ref writer, ref v);
    }

    void IMemoryPackFormatter.Deserialize(ref MemoryPackReader reader, scoped ref object? value)
    {
        var v = (T?)value;
        Deserialize(ref reader, ref v);
        value = v;
    }
}
