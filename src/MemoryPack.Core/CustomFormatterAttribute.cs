using MemoryPack.Internal;

namespace MemoryPack;

public abstract class CustomFormatterAttribute<T> : Attribute
{
    public abstract IMemoryPackFormatter<T> GetFormatter();
}

public sealed class Utf8StringFormatterAttribute : CustomFormatterAttribute<string>
{
    static readonly IMemoryPackFormatter<string> formatter = new Utf8Formatter();

    public override IMemoryPackFormatter<string> GetFormatter()
    {
        return formatter;
    }

    [Preserve]
    sealed class Utf8Formatter : MemoryPackFormatter<string>
    {
        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref string? value)
        {
            writer.WriteUtf8(value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref string? value)
        {
            value = reader.ReadString();
        }
    }
}

public sealed class BitPackFormatterAttribute : CustomFormatterAttribute<bool[]>
{
    static readonly IMemoryPackFormatter<bool[]> formatter = new BitPackFormatter();

    public override IMemoryPackFormatter<bool[]> GetFormatter()
    {
        return formatter;
    }

    [Preserve]
    sealed class BitPackFormatter : MemoryPackFormatter<IEnumerable<bool>>
    {
        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref bool[]? value)
        {
            foreach (var item in value)
            {
                // item
            }
            
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref IEnumerable<bool>? value)
        {
            
        }
    }
}
