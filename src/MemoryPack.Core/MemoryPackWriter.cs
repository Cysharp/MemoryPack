using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref partial struct MemoryPackWriter<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    ref TBufferWriter bufferWriter;
    ref byte bufferReference;
    int bufferLength;
    int advancedCount;

    public MemoryPackWriter(ref TBufferWriter writer)
    {
        this.bufferWriter = ref writer;
        this.bufferReference = ref Unsafe.NullRef<byte>();
        this.bufferLength = 0;
        this.advancedCount = 0;
    }

    // unsafe ctor, becareful to use.
    public MemoryPackWriter(ref TBufferWriter writer, byte[] firstBufferOfWriter)
    {
        this.bufferWriter = ref writer;
        this.bufferReference = ref MemoryMarshal.GetArrayDataReference(firstBufferOfWriter);
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

        return ref bufferReference;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    void RequestNewBuffer(int sizeHint)
    {
        if (advancedCount != 0)
        {
            bufferWriter.Advance(advancedCount);
            advancedCount = 0;
        }
        var span = bufferWriter.GetSpan(sizeHint);
        bufferReference = ref MemoryMarshal.GetReference(span);
        bufferLength = span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        bufferLength = bufferLength - count;
        // TODO: check safe advance?
        bufferReference = ref Unsafe.Add(ref bufferReference, count);
        advancedCount += count;
    }

    public void Flush()
    {
        if (advancedCount != 0)
        {
            bufferWriter.Advance(advancedCount);
            advancedCount = 0;
        }
        bufferReference = ref Unsafe.NullRef<byte>();
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
    public void WriteString(string? value)
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
    public void WriteUnmanagedArray<T>(T[]? value)
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
    public void DangerousWriteUnmanagedArray<T>(T[]? value)
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
        DangerousWriteUnmanagedSpan(ref value);
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

    public void WritePackable<T>(scoped ref T? value)
        where T : IMemoryPackable<T>
    {
        T.Serialize(ref this, ref value);
    }

    // non packable, get formatter dynamically.
    public void WriteObject<T>(scoped ref T? value)
    {
        var formatter = MemoryPackFormatterProvider.GetRequiredFormatter<T>();
        formatter.Serialize(ref this, ref value);
    }
}
