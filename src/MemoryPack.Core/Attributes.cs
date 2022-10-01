using System.Runtime.InteropServices;

namespace MemoryPack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackableAttribute : Attribute
{
    public GenerateType GenerateType { get; }
    public SerializeLayout SerializeLayout { get; }

    // ctor parameter count is used in MemoryPackGenerator.Parser TypeMeta for detect which ctor used.
    // if modify ctor, be careful.

    public MemoryPackableAttribute(GenerateType generateType = GenerateType.Object, SerializeLayout serializeLayout = SerializeLayout.Sequential)
    {
        this.GenerateType = generateType;
        this.SerializeLayout = serializeLayout;
    }

    // set SerializeLayout only allows Object
    public MemoryPackableAttribute(SerializeLayout serializeLayout)
    {
        this.GenerateType = GenerateType.Object;
        this.SerializeLayout = serializeLayout;
    }
}

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

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class MemoryPackUnionAttribute : Attribute
{
    public byte Tag { get; }
    public Type Type { get; }

    public MemoryPackUnionAttribute(byte tag, Type type)
    {
        this.Tag = tag;
        this.Type = type;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackAllowSerializeAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOrderAttribute : Attribute
{
    public int Order { get; }

    public MemoryPackOrderAttribute(int order)
    {
        this.Order = order;
    }
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
