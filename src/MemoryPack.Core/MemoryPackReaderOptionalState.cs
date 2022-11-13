using System.Collections.Concurrent;

namespace MemoryPack;

public static class MemoryPackReaderOptionalStatePool
{
    static readonly ConcurrentQueue<MemoryPackReaderOptionalState> queue = new ConcurrentQueue<MemoryPackReaderOptionalState>();

    public static MemoryPackReaderOptionalState Rent(MemoryPackSerializeOptions? options)
    {
        if (!queue.TryDequeue(out var state))
        {
            state = new MemoryPackReaderOptionalState();
        }

        state.Init(options);
        return state;
    }

    internal static void Return(MemoryPackReaderOptionalState writer)
    {
        writer.Reset();
        queue.Enqueue(writer);
    }
}

public sealed class MemoryPackReaderOptionalState : IDisposable
{
    readonly Dictionary<uint, object> RefToObject;
    public MemoryPackSerializeOptions Options { get; private set; }

    internal MemoryPackReaderOptionalState()
    {
        RefToObject = new Dictionary<uint, object>();
        Options = null!;
    }

    internal void Init(MemoryPackSerializeOptions? options)
    {
        Options = options ?? MemoryPackSerializeOptions.Default;
    }

    public void Reset()
    {
        RefToObject.Clear();
        Options = null!;
    }

    void IDisposable.Dispose()
    {
        Reset();
    }
}
