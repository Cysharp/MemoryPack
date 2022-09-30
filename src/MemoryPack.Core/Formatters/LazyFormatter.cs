namespace MemoryPack.Formatters;

public sealed class LazyFormatter<T> : MemoryPackFormatter<Lazy<T?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Lazy<T?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteValue(value.Value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Lazy<T?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 1) MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);

        var v = reader.ReadValue<T>();
        value = new Lazy<T?>(v);
    }
}
