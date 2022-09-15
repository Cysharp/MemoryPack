using MemoryPack.Internal;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    public static T? Deserialize<T>(ReadOnlySpan<byte> buffer)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (buffer.Length < Unsafe.SizeOf<T>())
            {
                ThrowHelper.ThrowInvalidRange(Unsafe.SizeOf<T>(), buffer.Length);
            }
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer));
        }

        var context = new MemoryPackReader(buffer);
        try
        {
            var formatter = MemoryPackFormatterProvider.GetRequiredFormatter<T>();

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
