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

public static partial class MemoryPackSerializer
{
    // Serialize

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(Type type, object? value, MemoryPackSerializeOptions? options = default)
    {
        var bufferWriter = threadStaticBufferWriter;
        if (bufferWriter == null)
        {
            bufferWriter = threadStaticBufferWriter = new ReusableLinkedArrayBufferWriter(useFirstBuffer: true, pinned: true);
        }

        try
        {
            var writer = new MemoryPackWriter(ref Unsafe.As<ReusableLinkedArrayBufferWriter, IBufferWriter<byte>>(ref bufferWriter), bufferWriter.DangerousGetFirstBuffer(), options ?? MemoryPackSerializeOptions.Default);
            Serialize(type, ref writer, value);
            return bufferWriter.ToArrayAndReset();
        }
        finally
        {
            bufferWriter.Reset();
        }
    }

    public static unsafe void Serialize(Type type, in IBufferWriter<byte> bufferWriter, object? value, MemoryPackSerializeOptions? options = default)
#if NET7_0_OR_GREATER
        
#else
        
#endif
    {
        var writer = new MemoryPackWriter(ref Unsafe.AsRef(bufferWriter), options ?? MemoryPackSerializeOptions.Default);
        Serialize(type, ref writer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Type type, ref MemoryPackWriter writer, object? value)
#if NET7_0_OR_GREATER
        
#else
        
#endif
    {
        writer.GetFormatter(type).Serialize(ref writer, ref value);
        writer.Flush();
    }

    public static async ValueTask SerializeAsync(Type type, Stream stream, object? value, MemoryPackSerializeOptions? options = default, CancellationToken cancellationToken = default)
    {
        var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            Serialize(tempWriter, value, options);
            await tempWriter.WriteToAndResetAsync(stream, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
        }
    }

    // Deserialize

    public static object? Deserialize(Type type, ReadOnlySpan<byte> buffer)
    {
        object? value = default;
        Deserialize(type, buffer, ref value);
        return value;
    }

    public static void Deserialize(Type type, ReadOnlySpan<byte> buffer, ref object? value)
    {
        var reader = new MemoryPackReader(buffer);
        try
        {
            reader.GetFormatter(type).Deserialize(ref reader, ref value);
        }
        finally
        {
            reader.Dispose();
        }
    }

    public static object? Deserialize(Type type, in ReadOnlySequence<byte> buffer)
    {
        object? value = default;
        Deserialize(type, buffer, ref value);
        return value;
    }

    public static void Deserialize(Type type, in ReadOnlySequence<byte> buffer, ref object? value)
    {
        var reader = new MemoryPackReader(buffer);
        try
        {
            reader.GetFormatter(type).Deserialize(ref reader, ref value);
        }
        finally
        {
            reader.Dispose();
        }
    }

    public static async ValueTask<object?> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
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
                    read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken).ConfigureAwait(false);
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
                return Deserialize(type, memory.Span);
            }
            else
            {
                var seq = builder.Build();
                var result = Deserialize(type, seq);
                return result;
            }
        }
        finally
        {
            builder.Reset();
        }
    }
}

}