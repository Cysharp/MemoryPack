using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace MemoryPack {

public static class MemoryPackWriterOptionalStatePool
{
    static readonly ConcurrentQueue<MemoryPackWriterOptionalState> queue = new ConcurrentQueue<MemoryPackWriterOptionalState>();

    public static MemoryPackWriterOptionalState Rent(MemoryPackSerializeOptions? options)
    {
        if (!queue.TryDequeue(out var state))
        {
            state = new MemoryPackWriterOptionalState();
        }

        state.Init(options);
        return state;
    }

    internal static void Return(MemoryPackWriterOptionalState writer)
    {
        writer.Reset();
        queue.Enqueue(writer);
    }
}

public sealed class MemoryPackWriterOptionalState : IDisposable
{
    readonly Dictionary<object, uint> ObjectToRef;
    public MemoryPackSerializeOptions Options { get; private set; }

    internal MemoryPackWriterOptionalState()
    {
        ObjectToRef = new Dictionary<object, uint>(ReferenceEqualityComparer.Instance);
        Options = null!;
    }

    internal void Init(MemoryPackSerializeOptions? options)
    {
        Options = options ?? MemoryPackSerializeOptions.Default;
    }

    public void Reset()
    {
        ObjectToRef.Clear();
        Options = null!;
    }

    void IDisposable.Dispose()
    {
        Reset();
    }

    // ReferenceEqualityComparer is exsits in .NET 6 but NetStandard 2.1 does not.
    sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        ReferenceEqualityComparer() { }

        public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}

}