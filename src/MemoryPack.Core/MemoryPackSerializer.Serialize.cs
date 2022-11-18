using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

#if NET7_0_OR_GREATER
using static MemoryMarshal;
using static GC;
#else
using static MemoryPack.Internal.MemoryMarshalEx;
#endif

public static partial class MemoryPackSerializer
{
    [ThreadStatic]
    static SerializerWriterThreadStaticState? threadStaticState;
    [ThreadStatic]
    static MemoryPackWriterOptionalState? threadStaticWriterOptionalState;

    public static byte[] Serialize<T>(in T? value, MemoryPackSerializeOptions? options = default)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var array = AllocateUninitializedArray<byte>(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref GetArrayDataReference(array), value);
            return array;
        }
#if NET7_0_OR_GREATER
        var typeKind = TypeHelpers.TryGetUnmanagedSZArrayElementSizeOrMemoryPackableFixedSize<T>(out var elementSize);
        if (typeKind == TypeHelpers.TypeKind.UnmanagedSZArray)
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
        else if (typeKind == TypeHelpers.TypeKind.FixedSizeMemoryPackable)
        {
            var buffer = new byte[(value == null) ? 1 : elementSize];
            var bufferWriter = new FixedArrayBufferWriter(buffer);
            var writer = new MemoryPackWriter<FixedArrayBufferWriter>(ref bufferWriter, buffer, MemoryPackWriterOptionalState.NullState);
            Serialize(ref writer, value);
            return bufferWriter.GetFilledBuffer();
        }
#endif

        var state = threadStaticState;
        if (state == null)
        {
            state = threadStaticState = new SerializerWriterThreadStaticState();
        }
        state.Init(options);

        try
        {
            var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref state.BufferWriter, state.BufferWriter.DangerousGetFirstBuffer(), state.OptionalState);
            Serialize(ref writer, value);
            return state.BufferWriter.ToArrayAndReset();
        }
        finally
        {
            state.Reset();
        }
    }

    public static unsafe void Serialize<T, TBufferWriter>(in TBufferWriter bufferWriter, in T? value, MemoryPackSerializeOptions? options = default)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
#endif
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var buffer = bufferWriter.GetSpan(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
            bufferWriter.Advance(Unsafe.SizeOf<T>());
            return;
        }
#if NET7_0_OR_GREATER
        var typeKind = TypeHelpers.TryGetUnmanagedSZArrayElementSizeOrMemoryPackableFixedSize<T>(out var elementSize);
        if (typeKind == TypeHelpers.TypeKind.UnmanagedSZArray)
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

        var state = threadStaticWriterOptionalState;
        if (state == null)
        {
            state = threadStaticWriterOptionalState = new MemoryPackWriterOptionalState();
        }
        state.Init(options);

        try
        {
            var writer = new MemoryPackWriter<TBufferWriter>(ref Unsafe.AsRef(bufferWriter), state);
            Serialize(ref writer, value);
        }
        finally
        {
            state.Reset();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<T, TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, in T? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
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

    sealed class SerializerWriterThreadStaticState
    {
        public ReusableLinkedArrayBufferWriter BufferWriter;
        public MemoryPackWriterOptionalState OptionalState;

        public SerializerWriterThreadStaticState()
        {
            BufferWriter = new ReusableLinkedArrayBufferWriter(useFirstBuffer: true, pinned: true);
            OptionalState = new MemoryPackWriterOptionalState();
        }

        public void Init(MemoryPackSerializeOptions? options)
        {
            OptionalState.Init(options);
        }

        public void Reset()
        {
            BufferWriter.Reset();
            OptionalState.Reset();
        }
    }
}
