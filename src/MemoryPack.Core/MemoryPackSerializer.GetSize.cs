using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    public static int GetSize<T>(in T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return Unsafe.SizeOf<T>();
        }
        if (TypeHelpers.TryGetUnmanagedSZArrayElementSize<T>(out var elementSize))
        {
            if (value == null)
            {
                return MemoryPackCode.NullCollectionData.Length;
            }

            var srcArray = ((Array)(object)value!);
            var length = srcArray.Length;
            if (length == 0)
            {
                return 4;
            }

            var dataSize = elementSize * length;
            return dataSize + 4;
        }


        var writer = new DoNothingMemoryPackWriter();
        var size = GetSize(ref writer, value);
        return size;
    }

    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSize<T>(ref DoNothingMemoryPackWriter writer, in T? value)
    {
        writer.WriteObject(value);
        return writer.WrittenCount;
    }
}
