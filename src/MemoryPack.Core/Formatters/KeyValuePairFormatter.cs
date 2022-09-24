namespace MemoryPack.Formatters;

public sealed class KeyValuePairFormatter<TKey, TValue> : MemoryPackFormatter<KeyValuePair<TKey?, TValue?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref KeyValuePair<TKey?, TValue?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<KeyValuePair<TKey?, TValue?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(2);
        writer.WriteObject(value.Key);
        writer.WriteObject(value.Value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref KeyValuePair<TKey?, TValue?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<KeyValuePair<TKey?, TValue?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 2) ThrowHelper.ThrowInvalidPropertyCount(2, count);

        value = new KeyValuePair<TKey?, TValue?>(
            reader.ReadObject<TKey>(),
            reader.ReadObject<TValue>()
        );
    }
}
