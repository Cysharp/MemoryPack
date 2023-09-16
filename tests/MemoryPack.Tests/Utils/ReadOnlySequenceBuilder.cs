using System;
using System.Linq;
using System.Buffers;

namespace MemoryPack.Tests.Utils;

public static class ReadOnlySequenceBuilder
{
    public static ReadOnlySequence<byte> Create(params byte[][] buffers)
    {
        var array = buffers.Select(x => new Segment(x)).ToArray();

        long running = 0;
        for (int i = 0; i < array.Length; i++)
        {
            var next = i < array.Length - 1 ? array[i + 1] : null;
            array[i].SetRunningIndexAndNext(running, next);
            running += array[i].Memory.Length;
        }

        var firstSegment = array[0];
        var lastSegment = array[array.Length - 1];
        var seq = new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
        return seq;
    }

    class Segment : ReadOnlySequenceSegment<byte>
    {
        public Segment(Memory<byte> buffer)
        {
            Memory = buffer;
        }

        internal void SetRunningIndexAndNext(long runningIndex, Segment? nextSegment)
        {
            RunningIndex = runningIndex;
            Next = nextSegment;
        }
    }
}
