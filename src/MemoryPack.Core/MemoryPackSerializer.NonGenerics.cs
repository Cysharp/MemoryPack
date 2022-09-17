using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    // Serialize

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(Type type, object? value)
    {
        var bufferWriter = threadStaticBufferWriter;
        if (bufferWriter == null)
        {
            bufferWriter = threadStaticBufferWriter = new ReusableLinkedArrayBufferWriter(useFirstBuffer: true, pinned: true);
        }

        try
        {
            var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufferWriter, bufferWriter.DangerousGetFirstBuffer());
            Serialize(type, ref writer, value);
            return bufferWriter.ToArrayAndReset();
        }
        finally
        {
            bufferWriter.Reset();
        }
    }

    public static unsafe void Serialize<TBufferWriter>(Type type, in TBufferWriter bufferWriter, object? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        var writer = new MemoryPackWriter<TBufferWriter>(ref Unsafe.AsRef(bufferWriter));
        Serialize(type, ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter>(Type type, ref MemoryPackWriter<TBufferWriter> writer, object? value)
    where TBufferWriter : IBufferWriter<byte>
    {
        MemoryPackFormatterProvider.GetFormatter(type).Serialize(ref writer, ref value);
        writer.Flush();
    }

    // TODO: SerializeAsync

    // TODO: Deserialize

}
