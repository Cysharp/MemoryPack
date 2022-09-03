using MemoryPack.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    [ThreadStatic]
    static SequentialBufferWriter? threadStaticBufferWriter;

    public static IMemoryPackFormatterProvider DefaultProvider { get; set; } = default!; // TODO: use defaultprovider.

    public static byte[] Serialize<T>(in T? value, IMemoryPackFormatterProvider? formatterProvider = default)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return SerializeUnmanaged(value);
        }

        var writer = threadStaticBufferWriter;
        if (writer == null)
        {
            writer = threadStaticBufferWriter = new SequentialBufferWriter(useFirstBuffer: true);
        }

        try
        {
            var context = new SerializationContext<SequentialBufferWriter>(writer, formatterProvider ?? DefaultProvider);
            var formatter = context.GetRequiredFormatter<T>(); // TODO: get from context or static abstract member?
            formatter.Serialize(ref context, ref Unsafe.AsRef(value));
            context.Flush();
            return writer.ToArrayAndReset();
        }
        finally
        {
            writer.Reset();
        }
    }

    public static void Serialize<T>(Stream stream, in T? value, IMemoryPackFormatterProvider? formatterProvider = default)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SerializeUnmanaged(stream, value);
            return;
        }

        using (var streamWriter = new SyncStreamBufferWriter(stream))
        {
            var context = new SerializationContext<SyncStreamBufferWriter>(streamWriter, formatterProvider ?? DefaultProvider);
            var formatter = context.GetRequiredFormatter<T>();
            formatter.Serialize(ref context, ref Unsafe.AsRef(value));
            context.Flush();
        }
    }

    public static T? Deserialize<T>(ReadOnlySpan<byte> buffer, IMemoryPackFormatterProvider? formatterProvider = default)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // TODO: DeserializeUnmanaged?
            // return SerializeUnmanaged(value);
        }

        var context = new DeserializationContext(buffer, formatterProvider ?? DefaultProvider);
        try
        {
            var formatter = context.GetRequiredFormatter<T>(); // TODO: get from context or static abstract member?

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