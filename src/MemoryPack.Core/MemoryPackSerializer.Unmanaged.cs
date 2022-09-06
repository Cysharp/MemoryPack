using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe byte[] DangerousSerializeUnmanaged<T>(in T value) // where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
        return buffer.ToArray();
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe void DangerousSerializeUnmanaged<TBufferWriter, T>(ref TBufferWriter bufferWriter, in T value) // where T : unmanaged
        where TBufferWriter : IBufferWriter<byte>
    {
        var buffer = bufferWriter.GetSpan(Unsafe.SizeOf<T>());
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
        bufferWriter.Advance(buffer.Length);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe void DangerousSerializeUnmanaged<T>(Stream stream, in T value) // where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
        stream.Write(buffer);
    }

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
}
