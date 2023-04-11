namespace MemoryPack.Generator;

// should synchronize with MemoryPack.Core.Attributes.cs GenerateType
public enum GenerateType
{
    Object,
    VersionTolerant,
    CircularReference,
    MultipleReferences,
    Collection,
    NoGenerate,

    // only used in Generator
    Union
}

public enum SerializeLayout
{
    Sequential, // default
    Explicit
}
