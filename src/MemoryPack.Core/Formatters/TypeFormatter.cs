namespace MemoryPack.Formatters;

public sealed class TypeFormatter : MemoryPackFormatter<Type>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Type? value)
    {
        // AssemblyQualifiedName?
        writer.WriteString(value?.FullName);
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
}
