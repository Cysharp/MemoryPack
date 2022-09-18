using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref partial struct MemoryPackReader
{
    ReadOnlySequence<byte> bufferSource;
    ref byte bufferReference;
    int bufferLength;
    byte[]? rentBuffer;
    int advancedCount;
    long restSequenceLength; // TODO:used for malformed length check

    public MemoryPackReader(in ReadOnlySequence<byte> sequence)
    {
        this.bufferSource = sequence.IsSingleSegment ? ReadOnlySequence<byte>.Empty : sequence;
        var span = sequence.FirstSpan;
        this.bufferReference = ref MemoryMarshal.GetReference(span);
        this.bufferLength = span.Length;
        this.advancedCount = 0;
        this.rentBuffer = null;
        this.restSequenceLength = sequence.Length;
    }

    public MemoryPackReader(ReadOnlySpan<byte> buffer)
    {
        this.bufferSource = ReadOnlySequence<byte>.Empty;
        this.bufferReference = ref MemoryMarshal.GetReference(buffer);
        this.bufferLength = buffer.Length;
        this.advancedCount = 0;
        this.rentBuffer = null;
        this.restSequenceLength = buffer.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref byte GetSpanReference(int sizeHint)
    {
        if (sizeHint <= bufferLength)
        {
            return ref bufferReference;
        }

        return ref GetNextSpan(sizeHint);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    ref byte GetNextSpan(int sizeHint)
    {
        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
            rentBuffer = null;
        }

        if (restSequenceLength == 0)
        {
            ThrowHelper.ThrowSequenceReachedEnd();
        }

        try
        {
            bufferSource = bufferSource.Slice(advancedCount);
        }
        catch (ArgumentOutOfRangeException)
        {
            ThrowHelper.ThrowSequenceReachedEnd();
        }

        advancedCount = 0;

        if (sizeHint <= restSequenceLength)
        {
            if (sizeHint <= bufferSource.FirstSpan.Length)
            {
                bufferReference = ref MemoryMarshal.GetReference(bufferSource.FirstSpan);
                bufferLength = bufferSource.FirstSpan.Length;
                return ref bufferReference;
            }

            rentBuffer = ArrayPool<byte>.Shared.Rent(sizeHint);
            bufferSource.Slice(0, sizeHint).CopyTo(rentBuffer);
            var span = rentBuffer.AsSpan(0, sizeHint);
            bufferReference = ref MemoryMarshal.GetReference(span);
            bufferLength = span.Length;
            return ref bufferReference;
        }

        ThrowHelper.ThrowSequenceReachedEnd();
        return ref bufferReference; // dummy.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        var rest = bufferLength - count;
        if (rest < 0)
        {
            ThrowHelper.ThrowInvalidAdvance();
        }

        bufferLength = rest;
        bufferReference = ref Unsafe.Add(ref bufferReference, count);
        advancedCount += count;
        restSequenceLength -= count;
    }

    public void Dispose()
    {
        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
        }
    }

    // helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadObjectHeader(out byte propertyCount)
    {
        propertyCount = GetSpanReference(1);
        Advance(1);
        return propertyCount != MemoryPackCode.NullObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadLengthHeader(out int length)
    {
        length = Unsafe.ReadUnaligned<int>(ref GetSpanReference(4));
        Advance(4);

        // If collection-length is larger than buffer-length, it is invalid data.
        if (restSequenceLength < length)
        {
            ThrowHelper.ThrowInsufficientBufferUnless(length);
        }

        return length != MemoryPackCode.NullLength;
    }

    public string? ReadString()
    {
        if (!TryReadUnmanagedSpan<char>(out var view, out var advanceLength))
        {
            return null;
        }

        if (view.Length == 0)
        {
            return "";
        }

        var str = new string(view);
        Advance(advanceLength);

        return str;
    }

    public T[]? ReadUnmanagedArray<T>()
        where T : unmanaged
    {
        return DangerousReadUnmanagedArray<T>();
    }

    // T: should be unamanged type
    public unsafe T[]? DangerousReadUnmanagedArray<T>()
    {
        if (!TryReadLengthHeader(out var length))
        {
            return null;
        }

        if (length == 0) return Array.Empty<T>();

        var size = length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(size);
        var dest = GC.AllocateUninitializedArray<T>(length);

        // TODO:this operation is maybe invalid, CreateSpan and CopyTo.
        Buffer.MemoryCopy(Unsafe.AsPointer(ref src), Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(dest)), size, size);
        Advance(size);

        return dest;
    }

    public unsafe void DangerousReadUnmanagedArray<T>(ref T[]? value)
    {
        if (!TryReadLengthHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            if (value != null && value.Length == 0) return;
            value = Array.Empty<T>();
            return;
        }

        var size = length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(size);

        if (value != null && value.Length == length)
        {
            // TODO:this operation is maybe invalid, CreateSpan and CopyTo.
            Buffer.MemoryCopy(Unsafe.AsPointer(ref src), Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(value)), size, size);
        }
        else
        {
            var dest = GC.AllocateUninitializedArray<T>(length);
            // TODO:this operation is maybe invalid, CreateSpan and CopyTo.
            Buffer.MemoryCopy(Unsafe.AsPointer(ref src), Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(dest)), size, size);
            value = dest;
        }
        Advance(size);
    }

    public bool TryReadUnmanagedSpan<T>(out ReadOnlySpan<T> view, out int advanceLength)
        where T : unmanaged
    {
        return DangerousTryReadUnmanagedSpan(out view, out advanceLength);
    }

    // T: should be unamanged type
    public bool DangerousTryReadUnmanagedSpan<T>(out ReadOnlySpan<T> view, out int advanceLength)
    {
        if (!TryReadLengthHeader(out var length))
        {
            view = default;
            advanceLength = 0;
            return false;
        }

        view = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, T>(ref GetSpanReference(length)), length);
        advanceLength = view.Length * Unsafe.SizeOf<T>();
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadPackable<T>(scoped ref T? value)
        where T : IMemoryPackable<T>
    {
        T.Deserialize(ref this, ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadPackable<T>()
        where T : IMemoryPackable<T>
    {
        T? value = default;
        T.Deserialize(ref this, ref value);
        return value;
    }

    // non packable, get formatter dynamically.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadObject<T>(scoped ref T? value)
    {
        MemoryPackFormatterProvider.GetFormatter<T>().Deserialize(ref this, ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadObject<T>()
    {
        T? value = default;
        MemoryPackFormatterProvider.GetFormatter<T>().Deserialize(ref this, ref value);
        return value;
    }
}
