using System.Buffers;

namespace MemoryPack;

public interface IMemoryPackFormatter<T>
{
    void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    void Deserialize(ref DeserializationContext context, ref T? value);
}
