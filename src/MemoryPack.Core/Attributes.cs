namespace MemoryPack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackableAttribute : Attribute
{
    public GenerateType GenerateType { get; }

    public MemoryPackableAttribute(GenerateType generateType = GenerateType.Object)
    {
        this.GenerateType = generateType;
    }
}

public enum GenerateType
{
    Object,
    Collection,
    NoGenerate
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class MemoryPackUnionAttribute : Attribute
{
    public byte Tag { get; set; }
    public Type Type { get; set; }

    public MemoryPackUnionAttribute(byte tag, Type type)
    {
        this.Tag = tag;
        this.Type = type;
    }
}

// initial design, does not provide this.
//[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
//public sealed class MemoryPackGenerateAttribute : Attribute
//{
//}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackAllowSerializeAttribute : Attribute
{
}

// similar naming as System.Text.Json attribtues
// https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonattribute

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackIgnoreAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackIncludeAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackConstructorAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOnSerializing : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOnSerialized : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOnDeserializing : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOnDeserialized : Attribute
{
}
