#if NET7_0_OR_GREATER

using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    public static byte[] Serialize<T, TContext>(in T? value, TContext context)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var array = GC.AllocateUninitializedArray<byte>(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetArrayDataReference(array), value);
            return array;
        }

        var typeKind = TypeHelpers.TryGetUnmanagedSZArrayElementSizeOrMemoryPackableFixedSize<T>(out var elementSize);
        if (typeKind == TypeHelpers.TypeKind.UnmanagedSZArray)
        {
            if (value is null)
            {
                return MemoryPackCode.NullCollectionData.ToArray();
            }

            var source = (Array)(object)value;
            if (source.Length == 0)
            {
                return new byte[4];
            }

            var dataSize = elementSize * source.Length;
            var destination = GC.AllocateUninitializedArray<byte>(dataSize + 4);
            ref var head = ref MemoryMarshal.GetArrayDataReference(destination);
            Unsafe.WriteUnaligned(ref head, source.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref head, 4), ref MemoryMarshal.GetArrayDataReference(source), (uint)dataSize);
            return destination;
        }

        if (typeKind == TypeHelpers.TypeKind.FixedSizeMemoryPackable)
        {
            var buffer = new byte[value is null ? 1 : elementSize];
            var fixedWriter = new FixedArrayBufferWriter(buffer);
            var writer = new MemoryPackWriter<FixedArrayBufferWriter>(ref fixedWriter, buffer, MemoryPackWriterOptionalState.NullState);
            TContext.Serialize(context, ref writer, ref Unsafe.AsRef(in value));
            writer.Flush();
            return fixedWriter.GetFilledBuffer();
        }

        var state = threadStaticState ??= new SerializerWriterThreadStaticState();
        state.Init(context.Options);
        try
        {
            var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref state.BufferWriter, state.BufferWriter.DangerousGetFirstBuffer(), state.OptionalState);
            TContext.Serialize(context, ref writer, ref Unsafe.AsRef(in value));
            writer.Flush();
            return state.BufferWriter.ToArrayAndReset();
        }
        finally
        {
            state.Reset();
        }
    }

    public static unsafe void Serialize<T, TContext, TBufferWriter>(in TBufferWriter bufferWriter, in T? value, TContext context)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
        where TBufferWriter : IBufferWriter<byte>
    {
        ArgumentNullException.ThrowIfNull(context);
        ref var bufferWriterRef = ref Unsafe.AsRef(in bufferWriter);

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var buffer = bufferWriterRef.GetSpan(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
            bufferWriterRef.Advance(Unsafe.SizeOf<T>());
            return;
        }

        var typeKind = TypeHelpers.TryGetUnmanagedSZArrayElementSizeOrMemoryPackableFixedSize<T>(out var elementSize);
        if (typeKind == TypeHelpers.TypeKind.UnmanagedSZArray)
        {
            if (value is null)
            {
                var span = bufferWriterRef.GetSpan(4);
                MemoryPackCode.NullCollectionData.CopyTo(span);
                bufferWriterRef.Advance(4);
                return;
            }

            var source = (Array)(object)value;
            var dataSize = elementSize * source.Length;
            var destination = bufferWriterRef.GetSpan(dataSize + 4);
            ref var head = ref MemoryMarshal.GetReference(destination);
            Unsafe.WriteUnaligned(ref head, source.Length);
            if (dataSize != 0)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref head, 4), ref MemoryMarshal.GetArrayDataReference(source), (uint)dataSize);
            }
            bufferWriterRef.Advance(dataSize + 4);
            return;
        }

        var optionalState = threadStaticWriterOptionalState ??= new MemoryPackWriterOptionalState();
        optionalState.Init(context.Options);
        try
        {
            var writer = new MemoryPackWriter<TBufferWriter>(ref bufferWriterRef, optionalState);
            TContext.Serialize(context, ref writer, ref Unsafe.AsRef(in value));
            writer.Flush();
        }
        finally
        {
            optionalState.Reset();
        }
    }

    public static async ValueTask SerializeAsync<T, TContext>(Stream stream, T? value, TContext context, CancellationToken cancellationToken = default)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
    {
        var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            Serialize(tempWriter, value, context);
            await tempWriter.WriteToAndResetAsync(stream, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
        }
    }

    public static T? Deserialize<T, TContext>(ReadOnlySpan<byte> buffer, TContext context)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
    {
        T? value = default;
        Deserialize(buffer, ref value, context);
        return value;
    }

    public static int Deserialize<T, TContext>(ReadOnlySpan<byte> buffer, ref T? value, TContext context)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (buffer.Length < Unsafe.SizeOf<T>())
            {
                MemoryPackSerializationException.ThrowInvalidRange(Unsafe.SizeOf<T>(), buffer.Length);
            }
            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer));
            return Unsafe.SizeOf<T>();
        }

        var state = threadStaticReaderOptionalState ??= new MemoryPackReaderOptionalState();
        state.Init(context.Options);
        var reader = new MemoryPackReader(buffer, state);
        try
        {
            TContext.Deserialize(context, ref reader, ref value);
            return reader.Consumed;
        }
        finally
        {
            reader.Dispose();
            state.Reset();
        }
    }

    public static T? Deserialize<T, TContext>(in ReadOnlySequence<byte> buffer, TContext context)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
    {
        T? value = default;
        Deserialize(buffer, ref value, context);
        return value;
    }

    public static int Deserialize<T, TContext>(in ReadOnlySequence<byte> buffer, ref T? value, TContext context)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var size = Unsafe.SizeOf<T>();
            if (buffer.Length < size)
            {
                MemoryPackSerializationException.ThrowInvalidRange(size, checked((int)buffer.Length));
            }

            if (buffer.FirstSpan.Length >= size)
            {
                value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer.FirstSpan));
                return size;
            }

            Span<byte> temporary = size <= 512 ? stackalloc byte[size] : new byte[size];
            buffer.Slice(0, size).CopyTo(temporary);
            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(temporary));
            return size;
        }

        var state = threadStaticReaderOptionalState ??= new MemoryPackReaderOptionalState();
        state.Init(context.Options);
        var reader = new MemoryPackReader(buffer, state);
        try
        {
            TContext.Deserialize(context, ref reader, ref value);
            return reader.Consumed;
        }
        finally
        {
            reader.Dispose();
            state.Reset();
        }
    }

    public static async ValueTask<T?> DeserializeAsync<T, TContext>(Stream stream, TContext context, CancellationToken cancellationToken = default)
        where TContext : MemoryPackSerializerContext, IMemoryPackSerializerContext<TContext, T>
    {
        if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var segment))
        {
            cancellationToken.ThrowIfCancellationRequested();
            T? value = default;
            var consumed = Deserialize(segment.AsSpan(checked((int)memoryStream.Position)), ref value, context);
            memoryStream.Seek(consumed, SeekOrigin.Current);
            return value;
        }

        var builder = ReusableReadOnlySequenceBuilderPool.Rent();
        try
        {
            var buffer = ArrayPool<byte>.Shared.Rent(65536);
            var offset = 0;
            while (true)
            {
                if (offset == buffer.Length)
                {
                    builder.Add(buffer, returnToPool: true);
                    buffer = ArrayPool<byte>.Shared.Rent(MathEx.NewArrayCapacity(buffer.Length));
                    offset = 0;
                }

                int read;
                try
                {
                    read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    throw;
                }

                offset += read;
                if (read == 0)
                {
                    builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
                    break;
                }
            }

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize<T, TContext>(memory.Span, context);
            }

            var sequence = builder.Build();
            return Deserialize<T, TContext>(sequence, context);
        }
        finally
        {
            builder.Reset();
        }
    }
}

#endif
