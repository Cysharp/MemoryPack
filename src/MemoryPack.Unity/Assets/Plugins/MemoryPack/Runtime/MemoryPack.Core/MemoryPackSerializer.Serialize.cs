using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack {

#if NET7_0_OR_GREATER
using static MemoryMarshal;
using static GC;
#else
using static MemoryPack.Internal.MemoryMarshalEx;
#endif

public static partial class MemoryPackSerializer
{
    [ThreadStatic]
    static ReusableLinkedArrayBufferWriter? threadStaticBufferWriter;

    public static byte[] Serialize<T>(in T? value, MemoryPackSerializeOptions? options = default)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var array = AllocateUninitializedArray<byte>(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref GetArrayDataReference(array), value);
            return array;
        }
#if NET7_0_OR_GREATER
        if (TypeHelpers.TryGetUnmanagedSZArrayElementSize<T>(out var elementSize))
        {
            if (value == null)
            {
                return MemoryPackCode.NullCollectionData.ToArray();
            }

            var srcArray = ((Array)(object)value!);
            var length = srcArray.Length;
            if (length == 0)
            {
                return new byte[4] { 0, 0, 0, 0 };
            }

            var dataSize = elementSize * length;
            var destArray = AllocateUninitializedArray<byte>(dataSize + 4);
            ref var head = ref MemoryMarshal.GetArrayDataReference(destArray);
            
            Unsafe.WriteUnaligned(ref head, length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref head, 4), ref MemoryMarshal.GetArrayDataReference(srcArray), (uint)dataSize);

            return destArray;
        }
#endif

        var bufferWriter = threadStaticBufferWriter;
        if (bufferWriter == null)
        {
            bufferWriter = threadStaticBufferWriter = new ReusableLinkedArrayBufferWriter(useFirstBuffer: true, pinned: true);
        }

        try
        {
            var writer = new MemoryPackWriter(ref Unsafe.As<ReusableLinkedArrayBufferWriter, IBufferWriter<byte>>(ref bufferWriter), bufferWriter.DangerousGetFirstBuffer(), options ?? MemoryPackSerializeOptions.Default);
            Serialize(ref writer, value);
            return bufferWriter.ToArrayAndReset();
        }
        finally
        {
            bufferWriter.Reset();
        }
    }

    public static unsafe void Serialize<T>(in IBufferWriter<byte> bufferWriter, in T? value, MemoryPackSerializeOptions? options = default)
#if NET7_0_OR_GREATER
        
#else
        
#endif
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var buffer = bufferWriter.GetSpan(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
            bufferWriter.Advance(buffer.Length);
            return;
        }
#if NET7_0_OR_GREATER
        if (TypeHelpers.TryGetUnmanagedSZArrayElementSize<T>(out var elementSize))
        {
            if (value == null)
            {
                var span = bufferWriter.GetSpan(4);
                MemoryPackCode.NullCollectionData.CopyTo(span);
                bufferWriter.Advance(4);
                return;
            }

            var srcArray = ((Array)(object)value!);
            var length = srcArray.Length;
            if (length == 0)
            {
                var span = bufferWriter.GetSpan(4);
                MemoryPackCode.ZeroCollectionData.CopyTo(span);
                bufferWriter.Advance(4);
                return;
            }
            var dataSize = elementSize * length;
            var destSpan = bufferWriter.GetSpan(dataSize + 4);
            ref var head = ref MemoryMarshal.GetReference(destSpan);

            Unsafe.WriteUnaligned(ref head, length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref head, 4), ref MemoryMarshal.GetArrayDataReference(srcArray), (uint)dataSize);

            bufferWriter.Advance(dataSize + 4);
            return;
        }
#endif

        var writer = new MemoryPackWriter(ref Unsafe.AsRef(bufferWriter), options ?? MemoryPackSerializeOptions.Default);
        Serialize(ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<T>(ref MemoryPackWriter writer, in T? value)
#if NET7_0_OR_GREATER
        
#else
        
#endif
    {
        writer.WriteValue(value);
        writer.Flush();
    }

    public static async ValueTask SerializeAsync<T>(Stream stream, T? value, MemoryPackSerializeOptions? options = default, CancellationToken cancellationToken = default)
    {
        var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            Serialize(tempWriter, value, options);
            await tempWriter.WriteToAndResetAsync(stream, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
        }
    }
}

}