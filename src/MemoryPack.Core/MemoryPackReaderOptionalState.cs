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
    readonly Dictionary<uint, object> refToObject;
    public MemoryPackSerializeOptions Options { get; private set; }

    internal MemoryPackReaderOptionalState()
    {
        refToObject = new Dictionary<uint, object>();
        Options = null!;
    }

    internal void Init(MemoryPackSerializeOptions? options)
    {
        Options = options ?? MemoryPackSerializeOptions.Default;
    }

    public object GetObjectReference(uint id)
    {
        if (refToObject.TryGetValue(id, out var value))
        {
            return value;
        }
        MemoryPackSerializationException.ThrowMessage("Object is not found in this reference id:" + id);
        return null!;
    }

    public void AddObjectReference(uint id, object value)
    {
        if (!refToObject.TryAdd(id, value))
        {
            MemoryPackSerializationException.ThrowMessage("Object is already added, id:" + id);
        }
    }

    public void Reset()
    {
        refToObject.Clear();
        Options = null!;
    }

    void IDisposable.Dispose()
    {
        Reset();
    }
}
