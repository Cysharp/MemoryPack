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

    public static byte[] Serialize<T>(in T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return SerializeUnmanaged(value);
        }

        var writer = threadStaticBufferWriter;
        if (writer == null)
        {
            writer = threadStaticBufferWriter = new SequentialBufferWriter();
        }

        try
        {
            var context = new SerializationContext<SequentialBufferWriter>(writer);
            IMemoryPackFormatter<T> formatter = default!; // TODO:get from provider? abstract static interface?
            formatter.Serialize(ref context, ref Unsafe.AsRef(value));
            context.Flush();
            return writer.ToArrayAndReset();
        }
        finally
        {
            writer.Reset();
        }
    }

    public static void Serialize<T>(Stream stream, in T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            SerializeUnmanaged(stream, value);
            return;
        }

        using (var streamWriter = new SyncStreamBufferWriter(stream))
        {
            var context = new SerializationContext<SyncStreamBufferWriter>(streamWriter);
            IMemoryPackFormatter<T> formatter = default!; // TODO:get from provider? abstract static interface?
            formatter.Serialize(ref context, ref Unsafe.AsRef(value));
            context.Flush();
        }
    }
}