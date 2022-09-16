using System.Buffers;

namespace MemoryPack;

public interface IMemoryPackFormatter<T>
{
    void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    void Deserialize(ref MemoryPackReader reader, scoped ref T? value);
}
