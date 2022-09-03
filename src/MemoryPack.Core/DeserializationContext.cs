using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref struct DeserializationContext
{
    const int NullLength = -1;
    const byte NullObject = 0;

    ReadOnlySequence<byte> sequenceSource;
    ReadOnlySpan<byte> buffer;
    byte[]? rentBuffer;

    public DeserializationContext(ReadOnlySequence<byte> sequence)
    {
        sequenceSource = sequence.IsSingleSegment ? default : sequence;
        buffer = sequence.FirstSpan;
        rentBuffer = null;
    }

    public DeserializationContext(ReadOnlySpan<byte> buffer)
    {
        sequenceSource = default;
        this.buffer = buffer;
        rentBuffer = null;
    }

    public ReadOnlySpan<byte> GetSpan(int sizeHint)
    {
        if (buffer.Length < sizeHint)
        {
            return buffer;
        }

        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
            rentBuffer = null;
        }

        sequenceSource = sequenceSource.Slice(buffer.Length);

        if (sequenceSource.Length < sizeHint)
        {
            if (sequenceSource.FirstSpan.Length < sizeHint)
            {
                buffer = sequenceSource.FirstSpan;
                return buffer;
            }

            rentBuffer = ArrayPool<byte>.Shared.Rent(sizeHint);
            sequenceSource.Slice(0, sizeHint).CopyTo(rentBuffer);
            buffer = rentBuffer.AsSpan(0, sizeHint);
            return buffer;
        }

        throw new Exception("TODO:message.");
    }

    public void Advance(int count)
    {
        buffer = buffer.Slice(count);
    }

    public bool ReadIsNull()
    {
        var span = GetSpan(1);
        var isNull = Unsafe.ReadUnaligned<byte>(ref MemoryMarshal.GetReference(span)) == NullObject;
        Advance(1);
        return isNull;
    }

    public int ReadLength()
    {
        var span = GetSpan(4);
        var length = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(span));
        Advance(4);
        return length;
    }

    public string? ReadString()
    {
        var length = ReadLength();
        if (length == NullLength) return null;

        var span = GetSpan(length);
        var charSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, char>(ref MemoryMarshal.GetReference(span)), span.Length / 2);
        var str = new string(charSpan);

        Advance(length);

        return str;
    }

    public void Dispose()
    {
        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
        }
    }
}