using System.Buffers;

namespace MemoryPack;

public interface IMemoryPackFormatter
{
}

public interface IMemoryPackFormatter<T> : IMemoryPackFormatter
{
    void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> context, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    void Deserialize(ref MemoryPackReader context, scoped ref T? value);
}
