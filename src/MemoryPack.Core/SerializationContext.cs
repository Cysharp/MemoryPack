using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref struct SerializationContext<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    const int NullLength = -1;
    const byte NullObject = 0;

    readonly TBufferWriter writer; // TODO:ref field
    Span<byte> buffer;

    public int TotalWritten { get; private set; }

    public SerializationContext(TBufferWriter writer)
    {
        this.writer = writer;
        buffer = default;
        TotalWritten = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint)
    {
        if (buffer.Length < sizeHint)
        {
            ProvideNewBuffer(sizeHint);
        }
        return buffer;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    void ProvideNewBuffer(int size)
    {
        buffer = writer.GetSpan(size - buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullLength()
    {
        var span = GetSpan(4);
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), NullLength);
        Advance(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullObject()
    {
        var span = GetSpan(1);
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), NullObject);
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
        var span = GetSpan(src.Length + 4);

        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), src.Length);
        src.CopyTo(span.Slice(4));

        Advance(src.Length + 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        writer.Advance(count);
        buffer = buffer.Slice(count);
        TotalWritten += count;
    }
}