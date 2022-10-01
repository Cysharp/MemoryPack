namespace MemoryPack.Generator;

// should synchronize with MemoryPack.Core.Attributes.cs GenerateType
public enum GenerateType
{
    Object,
    Collection,
    NoGenerate
}

public enum SerializeLayout
{
    Sequential, // default
    Explicit
}
