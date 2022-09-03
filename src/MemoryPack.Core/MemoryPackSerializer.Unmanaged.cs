using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public static class MemoryPackSerializer
{
    [SkipLocalsInit]
    public static unsafe byte[] Serialize<T>(in T value)
        where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[sizeof(T)];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
        return buffer.ToArray();
    }

    [SkipLocalsInit]
    public static unsafe int Serialize<TBufferWriter, T>(TBufferWriter bufferWriter, in T value)
        where TBufferWriter : IBufferWriter<byte>
        where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[sizeof(T)];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);

        var dest = bufferWriter.GetSpan(buffer.Length);
        buffer.CopyTo(dest);

        bufferWriter.Advance(buffer.Length);

        return sizeof(T);
    }

    [SkipLocalsInit]
    public static unsafe int Serialize<T>(Stream stream, in T value)
        where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[sizeof(T)];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);

        stream.Write(buffer);

        return sizeof(T);
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
