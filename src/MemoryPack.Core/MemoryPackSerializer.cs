using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    [ThreadStatic]
    static SequentialBufferWriter? threadStaticBufferWriter;

    public static byte[] Serialize<T>(in T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return DangerousSerializeUnmanaged(value);
        }

        var writer = threadStaticBufferWriter;
        if (writer == null)
        {
            writer = threadStaticBufferWriter = new SequentialBufferWriter(useFirstBuffer: true, pinned: true);
        }

        try
        {
            var context = new SerializationContext<SequentialBufferWriter>(writer, writer.DangerousGetFirstBuffer());
            Serialize(ref context, value);
            return writer.ToArrayAndReset();
        }
        finally
        {
            writer.Reset();
        }
    }

    public static void Serialize<T, TBufferWriter>(ref TBufferWriter bufferWriter, in T? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            DangerousSerializeUnmanaged(ref bufferWriter, value);
            return;
        }

        var context = new SerializationContext<TBufferWriter>(bufferWriter);
        Serialize(ref context, value);
    }

    public static void Serialize<T>(Stream stream, in T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            DangerousSerializeUnmanaged(stream, value);
            return;
        }

        using (var streamWriter = new SyncStreamBufferWriter(stream))
        {
            var context = new SerializationContext<SyncStreamBufferWriter>(streamWriter);
            Serialize(ref context, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<T, TBufferWriter>(ref SerializationContext<TBufferWriter> context, in T? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        context.WriteObject(ref Unsafe.AsRef(value));
        context.Flush();
    }

    public static T? Deserialize<T>(ReadOnlySpan<byte> buffer)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // TODO: DeserializeUnmanaged?
            // return SerializeUnmanaged(value);
        }

        var context = new DeserializationContext(buffer);
        try
        {
            var formatter = MemoryPackFormatterProvider.GetRequiredFormatter<T>(); // TODO: get from context or static abstract member?

            T? value = default;
            formatter.Deserialize(ref context, ref value);
            return value;
        }
        finally
        {
            context.Dispose();
        }
    }
}
