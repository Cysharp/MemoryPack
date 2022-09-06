using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref struct SerializationContext<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    readonly TBufferWriter bufferWriter; // TODO: ref field?
    Span<byte> buffer; // TODO: ref byte bufferReference
    int bufferLength;
    int advancedCount;

    public SerializationContext(TBufferWriter writer)
    {
        this.bufferWriter = writer;
        this.buffer = default;
        this.bufferLength = 0;
        this.advancedCount = 0;
    }

    // unsafe ctor, becareful to use.
    public SerializationContext(TBufferWriter writer, byte[] firstBufferOfWriter)
    {
        this.bufferWriter = writer;
        this.buffer = firstBufferOfWriter;
        this.bufferLength = firstBufferOfWriter.Length;
        this.advancedCount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref byte GetSpanReference(int sizeHint)
    {
        if (bufferLength < sizeHint)
        {
            RequestNewBuffer(sizeHint);
        }

        // TODO: return ref bufferReference;
        return ref MemoryMarshal.GetReference(buffer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    void RequestNewBuffer(int size)
    {
        if (advancedCount != 0)
        {
            bufferWriter.Advance(advancedCount);
            advancedCount = 0;
        }
        buffer = bufferWriter.GetSpan(size - buffer.Length);
        bufferLength = buffer.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        buffer = buffer.Slice(count); // TODO: ref Unsafe.Add(ref bufferReference, count)
        bufferLength = buffer.Length;
        advancedCount += count;
    }

    public void Flush()
    {
        if (advancedCount != 0)
        {
            bufferWriter.Advance(advancedCount);
            advancedCount = 0;
        }
        buffer = default;
        bufferLength = 0;
    }

    // helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullObjectHeader()
    {
        GetSpanReference(1) = MemoryPackCode.NullObject;
        Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteObjectHeader(byte propertyCount)
    {
        if (propertyCount >= MemoryPackCode.Reserved1)
        {
            // TODO: throws invalid property length?
        }
        GetSpanReference(1) = propertyCount;
        Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnionHeader(byte tag)
    {
        ref var spanRef = ref GetSpanReference(2);
        spanRef = MemoryPackCode.Union;
        Unsafe.Add(ref spanRef, 1) = tag;
        Advance(2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLengthHeader(int length)
    {
        Unsafe.WriteUnaligned(ref GetSpanReference(4), length);
        Advance(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullLengthHeader()
    {
        Unsafe.WriteUnaligned(ref GetSpanReference(4), MemoryPackCode.NullLength);
        Advance(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(ref string? value)
    {
        if (value == null)
        {
            WriteNullLengthHeader();
            return;
        }

        var span = value.AsSpan();
        WriteUnmanagedSpan(ref span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedArray<T>(ref T[]? value)
        where T : unmanaged
    {
        if (value == null)
        {
            WriteNullLengthHeader();
            return;
        }

        var span = new ReadOnlySpan<T>(value);
        WriteUnmanagedSpan(ref span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedArray<T>(ref T[]? value)
    {
        if (value == null)
        {
            WriteNullLengthHeader();
            return;
        }

        var span = new ReadOnlySpan<T>(value);
        DangerousWriteUnmanagedSpan(ref span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedSpan<T>(ref ReadOnlySpan<T> value)
        where T : unmanaged
    {
        var src = MemoryMarshal.AsBytes(value);
        ref var spanRef = ref GetSpanReference(src.Length + 4);

        Unsafe.WriteUnaligned(ref spanRef, value.Length);
        src.CopyTo(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref spanRef, 4), src.Length));

        Advance(src.Length + 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedSpan<T>(ref ReadOnlySpan<T> value)
    {
        // MemoryMarshal.AsBytes(value);
        var src = MemoryMarshal.CreateReadOnlySpan<byte>(
            ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
            checked(value.Length * Unsafe.SizeOf<T>()));

        ref var spanRef = ref GetSpanReference(src.Length + 4);

        Unsafe.WriteUnaligned(ref spanRef, value.Length);
        src.CopyTo(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref spanRef, 4), src.Length));

        Advance(src.Length + 4);
    }

    public void WritePackable<T>(ref T? value)
        where T : IMemoryPackable<T>
    {
        T.Serialize(ref this, ref value);
    }

    // non packable, get formatter dynamically.
    public void WriteObject<T>(ref T? value)
    {
        var formatter = MemoryPackFormatterProvider.GetRequiredFormatter<T>();
        formatter.Serialize(ref this, ref value);
    }
}
