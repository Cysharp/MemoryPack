using System.Buffers;

namespace MemoryPack;

public interface IMemoryPackFormatterRegister
{
    static abstract void RegisterFormatter();
}

public interface IMemoryPackable<T> : IMemoryPackFormatterRegister
{
    static abstract void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    static abstract void Deserialize(ref MemoryPackReader reader, scoped ref T? value);
}
