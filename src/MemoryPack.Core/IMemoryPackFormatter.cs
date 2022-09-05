using System.Buffers;

namespace MemoryPack;

public interface IMemoryPackFormatter
{
}

public interface IMemoryPackFormatter<T> : IMemoryPackFormatter
{
    void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref T? value)
        where TBufferWriter : IBufferWriter<byte>;
    void Deserialize(ref DeserializationContext context, ref T? value);
}
