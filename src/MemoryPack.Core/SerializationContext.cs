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

    public int TotalWritten { get; private set; } // TODO:require this?

    public SerializationContext(TBufferWriter writer)
    {
        this.bufferWriter = writer;
        this.buffer = default;
        this.bufferLength = 0;
        this.advancedCount = 0;
        this.TotalWritten = 0;
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
        TotalWritten += count;
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
    public void WriteNullLength()
    {
        Unsafe.WriteUnaligned(ref GetSpanReference(4), MemoryPackCode.NullLength);
        Advance(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullObject()
    {
        Unsafe.WriteUnaligned(ref GetSpanReference(1), MemoryPackCode.NullObject);
        Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLength(int length)
    {
        Unsafe.WriteUnaligned(ref GetSpanReference(4), length);
        Advance(4);
    }

    public void WriteString(string? value)
    {
        if (value == null)
        {
            WriteNullLength();
            return;
        }

        var src = MemoryMarshal.AsBytes(value.AsSpan());
        ref var spanRef = ref GetSpanReference(src.Length + 4);

        Unsafe.WriteUnaligned(ref spanRef, src.Length);
        src.CopyTo(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref spanRef, 4), src.Length));

        Advance(src.Length + 4);
    }
}