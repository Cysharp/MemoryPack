using BenchmarkDotNet.Attributes;
using MemoryPack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Micro;

public class SpanSliceTest
{
    byte[] raw = new byte[1000];
    const int SliceCount = 100;

    [Benchmark]
    public void SpanSlice()
    {
        var writer = new SpanWriter(raw);
        for (int i = 0; i < SliceCount; i++)
        {
            writer.Advance(10);
        }
    }

    [Benchmark]
    public void RefReferenceAdd()
    {
        var writer = new SpanRefWriter(raw);
        for (int i = 0; i < SliceCount; i++)
        {
            writer.Advance(10);
        }
    }

    [Benchmark]
    public void ReadOnlySequenceSlice()
    {
        var seq = new ReadOnlySequence<byte>(raw);
        for (int i = 0; i < SliceCount; i++)
        {
            seq = seq.Slice(10);
        }
    }
}

public ref struct SpanWriter
{
    Span<byte> raw;

    public SpanWriter(byte[] buffer)
    {
        raw = buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        raw = raw.Slice(count);
    }
}

public ref struct SpanRefWriter
{
    ref byte bufferReference;
    int bufferLength;

    public SpanRefWriter(byte[] buffer)
    {
        bufferReference = ref MemoryMarshal.GetArrayDataReference(buffer);
        bufferLength = buffer.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        var rest = bufferLength - count;
        if (rest < 0)
        {
            Throw();
        }

        bufferLength = rest;
        bufferReference = ref Unsafe.Add(ref bufferReference, count);
    }

    [DoesNotReturn]
    void Throw()
    {
        throw new InvalidOperationException();
    }
}
