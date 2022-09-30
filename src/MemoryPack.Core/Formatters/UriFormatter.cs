﻿using System.Buffers;

namespace MemoryPack.Formatters;

public sealed class UriFormatter : MemoryPackFormatter<Uri>
{
    // treat as a string(OriginalString).

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Uri? value)
    {
        writer.WriteString(value?.OriginalString);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Uri? value)
    {
        var str = reader.ReadString();
        if (str == null)
        {
            value = null;
        }
        else
        {
            value = new Uri(str, UriKind.RelativeOrAbsolute);
        }
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Uri? value)
    {
        throw new NotImplementedException();
    }
}
