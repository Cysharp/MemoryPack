using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref struct DeserializationContext
{
    const int NullLength = -1;
    const byte NullObject = 0;

    ReadOnlySequence<byte> bufferSource; // TODO:ref?
    ReadOnlySpan<byte> buffer; // TODO:ref byte bufferReference
    int bufferLength;
    byte[]? rentBuffer;

    public DeserializationContext(ReadOnlySequence<byte> sequence)
    {
        this.bufferSource = sequence.IsSingleSegment ? default : sequence;
        this.buffer = sequence.FirstSpan;
        this.bufferLength = buffer.Length;
        this.rentBuffer = null;
    }

    public DeserializationContext(ReadOnlySpan<byte> buffer)
    {
        this.bufferSource = default;
        this.buffer = buffer;
        this.bufferLength = buffer.Length;
        this.rentBuffer = null;
    }

    public ref byte GetSpanReference(int sizeHint)
    {
        if (sizeHint <= buffer.Length)
        {
            // TODO: return ref bufferReference
            return ref MemoryMarshal.GetReference(buffer);
        }

        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
            rentBuffer = null;
        }

        bufferSource = bufferSource.Slice(buffer.Length);

        if (bufferSource.Length < sizeHint)
        {
            if (bufferSource.FirstSpan.Length < sizeHint)
            {
                buffer = bufferSource.FirstSpan;
                bufferLength = buffer.Length;
                return ref MemoryMarshal.GetReference(buffer);
            }

            rentBuffer = ArrayPool<byte>.Shared.Rent(sizeHint);
            bufferSource.Slice(0, sizeHint).CopyTo(rentBuffer);
            buffer = rentBuffer.AsSpan(0, sizeHint);
            bufferLength = buffer.Length;
            return ref MemoryMarshal.GetReference(buffer);
        }

        throw new Exception("TODO:message.");
    }

    public void Advance(int count)
    {
        buffer = buffer.Slice(count); // TODO: ref Unsafe.Add(ref bufferReference, count)
        bufferLength = buffer.Length;
    }

    public void Dispose()
    {
        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
        }
    }

    // helpers

    public bool ReadIsNull()
    {
        var isNull = Unsafe.ReadUnaligned<byte>(ref GetSpanReference(1)) == NullObject;
        Advance(1);
        return isNull;
    }

    public int ReadLength()
    {
        var length = Unsafe.ReadUnaligned<int>(ref GetSpanReference(4));
        Advance(4);
        return length;
    }

    public string? ReadString()
    {
        var length = ReadLength();
        if (length == NullLength) return null;

        var charSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, char>(ref GetSpanReference(length)), length / 2);
        var str = new string(charSpan);

        Advance(length);

        return str;
    }
}