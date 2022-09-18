using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MemoryPack.Formatters;

public sealed class VersionFormatter : MemoryPackFormatter<Version>
{
    // Serialize as [Major, Minor, Build, Revision]

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Version? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteUnmanagedWithObjectHeader(4, value.Major, value.Minor, value.Build, value.Revision);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Version? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 4) ThrowHelper.ThrowInvalidPropertyCount(4, count);

        reader.ReadUnmanaged(out int major, out int minor, out int build, out int revision);
        value = new Version(major, minor, build, revision);



        //var __value;
        var __major = reader.ReadString();
        var __minor = reader.ReadObject<int>();
        var __build = reader.ReadObject<int>();
        var __revision = reader.ReadObject<int>();

        // reader.ReadPackable<int>(ref var    v);

    }
}
