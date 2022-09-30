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
        T? value = default;
        Deserialize(buffer, ref value);
        return value;
    }

    public static void Deserialize<T>(ReadOnlySpan<byte> buffer, ref T? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (buffer.Length < Unsafe.SizeOf<T>())
            {
                MemoryPackSerializationException.ThrowInvalidRange(Unsafe.SizeOf<T>(), buffer.Length);
            }
            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer));
            return;
        }

        var reader = new MemoryPackReader(buffer);
        try
        {
            reader.ReadValue(ref value);
        }
        finally
        {
            reader.Dispose();
        }
    }

    public static T? Deserialize<T>(in ReadOnlySequence<byte> buffer)
    {
        T? value = default;
        Deserialize<T>(buffer, ref value);
        return value;
    }

    public static void Deserialize<T>(in ReadOnlySequence<byte> buffer, ref T? value)
    {
        var reader = new MemoryPackReader(buffer);
        try
        {
            reader.ReadValue(ref value);
        }
        finally
        {
            reader.Dispose();
        }
    }

    public static async ValueTask<T?> DeserializeAsync<T>(Stream stream)
    {
        var builder = ReusableReadOnlySequenceBuilderPool.Rent();
        try
        {
            var buffer = ArrayPool<byte>.Shared.Rent(65536); // initial 64K
            var offset = 0;
            do
            {
                if (offset == buffer.Length)
                {
                    builder.Add(buffer, returnToPool: true);
                    buffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                    offset = 0;
                }

                int read = 0;
                try
                {
                    read = await stream.ReadAsync(buffer, offset, buffer.Length - offset).ConfigureAwait(false);
                }
                catch
                {
                    // buffer is not added in builder, so return here.
                    ArrayPool<byte>.Shared.Return(buffer);
                    throw;
                }

                offset += read;

                if (read == 0)
                {
                    builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
                    break;

                }
            } while (true);

            // If single buffer, we can avoid ReadOnlySequence build cost.
            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize<T>(memory.Span);
            }
            else
            {
                var seq = builder.Build();
                var result = Deserialize<T>(seq);
                return result;
            }
        }
        finally
        {
            builder.Reset();
        }
    }
}
