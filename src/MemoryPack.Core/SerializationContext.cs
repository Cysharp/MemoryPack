using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref struct SerializationContext<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    const int NullLength = -1;
    const byte NullObject = 0;

    readonly TBufferWriter bufferWriter; // TODO: ref field?
    Span<byte> buffer; // TODO: ref byte bufferReference
    int bufferLength;

    public int TotalWritten { get; private set; }

    public SerializationContext(TBufferWriter writer)
    {
        this.bufferWriter = writer;
        this.buffer = default;
        this.bufferLength = 0;
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
        buffer = bufferWriter.GetSpan(size - buffer.Length);
        bufferLength = buffer.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        bufferWriter.Advance(count);
        buffer = buffer.Slice(count); // TODO: ref Unsafe.Add(ref bufferReference, count)
        bufferLength = buffer.Length;
        TotalWritten += count;
    }

    // helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullLength()
    {
        Unsafe.WriteUnaligned(ref GetSpanReference(4), NullLength);
        Advance(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullObject()
    {
        Unsafe.WriteUnaligned(ref GetSpanReference(1), NullObject);
        Advance(1);
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