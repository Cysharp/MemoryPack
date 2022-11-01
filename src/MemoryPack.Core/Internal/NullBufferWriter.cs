using MemoryPack.Formatters;
using System.Buffers;

namespace MemoryPack.Internal;

// Internal use for AOT safety(dummy write)

public sealed class NullBufferWriter : IBufferWriter<byte>
{
    public static readonly NullBufferWriter Instance = new NullBufferWriter();

    byte[] dummyBuffer = new byte[256];

    NullBufferWriter()
    {
    }

    public void Advance(int count)
    {
        if (!MemoryPackFormatterProvider.IsRegistered<KeyValuePair<int, string>>())
        {
            var f = new KeyValuePairFormatter<int, string>();
            var bufferWriter = NullBufferWriter.Instance;
            var writer = new MemoryPackWriter<NullBufferWriter>(ref bufferWriter, MemoryPackSerializeOptions.Default);
            var value = default(KeyValuePair<int, string>);
            f.Serialize(ref writer, ref value!);
        }
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        lock (dummyBuffer)
        {
            if (dummyBuffer.Length < sizeHint)
            {
                dummyBuffer = new byte[sizeHint];
            }
        }
        return dummyBuffer;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return GetMemory(sizeHint).Span;
    }
}
