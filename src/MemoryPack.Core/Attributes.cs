namespace MemoryPack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackableAttribute : Attribute
{
    public bool AllowPrivate { get; }

    public MemoryPackableAttribute(bool allowPrivate = false)
    {
        AllowPrivate = allowPrivate;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class UnionAttribute : Attribute
{
    public byte Tag { get; set; }
    public Type Type { get; set; }

    public UnionAttribute(byte tag, Type type)
    {
        this.Tag = tag;
        this.Type = type;
    }
}
