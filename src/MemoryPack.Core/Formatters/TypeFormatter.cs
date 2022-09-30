using System.Text.RegularExpressions;

namespace MemoryPack.Formatters;

public sealed partial class TypeFormatter : MemoryPackFormatter<Type>
{
    // Remove Version, Culture, PublicKeyToken from AssemblyQualifiedName.
    // Result will be "TypeName, Assembly"
    // see:http://msdn.microsoft.com/en-us/library/w3f99sx1.aspx


    [GeneratedRegex(@", Version=\d+.\d+.\d+.\d+, Culture=[\w-]+, PublicKeyToken=(?:null|[a-f0-9]{16})")]
    private static partial Regex ShortTypeNameRegex();

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Type? value)
    {
        var full = value?.AssemblyQualifiedName;
        if (full == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var shortName = ShortTypeNameRegex().Replace(full, "");
        writer.WriteString(shortName);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Type? value)
    {
        var typeName = reader.ReadString();
        if (typeName == null)
        {
            value = null;
            return;
        }

        value = Type.GetType(typeName, throwOnError: true);
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Type? value)
    {
        throw new NotImplementedException();
    }
}
