using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    [ThreadStatic]
    static LinkedArrayBufferWriter? threadStaticBufferWriter;

    public static unsafe byte[] Serialize<T>(in T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var array = GC.AllocateUninitializedArray<byte>(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetArrayDataReference(array), value);
            return array;
        }
        if (TypeHelpers.TryGetUnmanagedSZArrayElementSize<T>(out var elementSize))
        {
            if (value == null)
            {
                return MemoryPackCode.NullCollection.ToArray();
            }

            var srcArray = ((Array)(object)value!);
            var length = srcArray.Length;
            if (length == 0)
            {
                return new byte[4] { 0, 0, 0, 0 };
            }

            var dataSize = elementSize * length;
            var destArray = GC.AllocateUninitializedArray<byte>(dataSize + 4);

            ref var head = ref MemoryMarshal.GetArrayDataReference(destArray);
            Unsafe.WriteUnaligned(ref head, length);
            Buffer.MemoryCopy(
                source: Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(srcArray)),
                destination: Unsafe.AsPointer(ref Unsafe.Add(ref head, 4)),
                destinationSizeInBytes: dataSize,
                sourceBytesToCopy: dataSize);
            return destArray;
        }

        var writer = threadStaticBufferWriter;
        if (writer == null)
        {
            writer = threadStaticBufferWriter = new LinkedArrayBufferWriter(useFirstBuffer: true, pinned: true);
        }

        try
        {
            var context = new MemoryPackWriter<LinkedArrayBufferWriter>(ref writer, writer.DangerousGetFirstBuffer());
            SerializeCore(ref context, value);
            return writer.ToArrayAndReset();
        }
        finally
        {
            writer.Reset();
        }
    }

    public static unsafe void Serialize<T, TBufferWriter>(in TBufferWriter bufferWriter, in T? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var buffer = bufferWriter.GetSpan(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
            bufferWriter.Advance(buffer.Length);
            return;
        }
        if (TypeHelpers.TryGetUnmanagedSZArrayElementSize<T>(out var elementSize))
        {
            if (value == null)
            {
                var span = bufferWriter.GetSpan(4);
                MemoryPackCode.NullCollection.CopyTo(span);
                bufferWriter.Advance(4);
                return;
            }

            var srcArray = ((Array)(object)value!);
            var length = srcArray.Length;
            if (length == 0)
            {
                var span = bufferWriter.GetSpan(4);
                MemoryPackCode.ZeroCollection.CopyTo(span);
                bufferWriter.Advance(4);
                return;
            }
            var dataSize = elementSize * length;
            var destSpan = bufferWriter.GetSpan(dataSize + 4);
            ref var head = ref MemoryMarshal.GetReference(destSpan);
            Unsafe.WriteUnaligned(ref head, length);
            Buffer.MemoryCopy(
                source: Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(srcArray)),
                destination: Unsafe.AsPointer(ref Unsafe.Add(ref head, 4)),
                destinationSizeInBytes: dataSize,
                sourceBytesToCopy: dataSize);
            bufferWriter.Advance(dataSize + 4);
            return;
        }

        var context = new MemoryPackWriter<TBufferWriter>(ref Unsafe.AsRef(bufferWriter));
        SerializeCore(ref context, value);
    }

    [SkipLocalsInit]
    public static void Serialize<T>(Stream stream, in T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
            stream.Write(buffer);
            return;
        }

        var streamWriter = new SyncStreamBufferWriter(stream);
        try
        {
            var context = new MemoryPackWriter<SyncStreamBufferWriter>(ref streamWriter);
            SerializeCore(ref context, value);
        }
        finally
        {
            streamWriter.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SerializeCore<T, TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, in T? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteObject(ref Unsafe.AsRef(value));
        writer.Flush();
    }

    // TODO: is this api ok?
    public static async ValueTask<int> SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default)
        where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        var buffer = ArrayPool<byte>.Shared.Rent(size);
        try
        {
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer.AsSpan()), value);
            await stream.WriteAsync(buffer.AsMemory(0, size), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return size;
    }

    // TODO:NonGenerics?
}
