using System.Buffers;

namespace MemoryPack;

public interface IMemoryPackable<T>
{
    static abstract void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> context, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    static abstract void Deserialize(ref MemoryPackReader context, scoped ref T? value);
}
