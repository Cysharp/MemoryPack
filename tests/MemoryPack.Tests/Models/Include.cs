#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169

namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class Include
{
    public int PublicProp { get; set; }
    public int PublicField;

    [MemoryPackIgnore]
    public string? NoInclude { get; set; }

    public string? PrivateSet { get; private set; }
    public string? PrivateGet { private get; set; }

    [MemoryPackInclude]
    private string? PrivateProp { get; set; }
    [MemoryPackInclude]
    private int PrivateField;

    public void SetAll(int publicProp, int publicFIeld, string privateSet, string privateGet, string privateProp, int privateField)
    {
        this.PublicProp = publicProp;
        this.PublicField = publicFIeld;
        this.PrivateSet = privateSet;
        this.PrivateGet = privateGet;
        this.PrivateProp = privateProp;
        this.PrivateField = privateField;
    }

    public (int, int, string?, string?, string?, int) GetAll()
    {
        return (PublicProp, PublicField, PrivateSet, PrivateGet, PrivateProp, PrivateField);
    }
}

[MemoryPackable]
public partial class NoInclude
{
    public int PublicProp { get; set; }
    public int PublicField;

    public string? PrivateSet { get; private set; }
    public string? PrivateGet { private get; set; }

    private string? PrivateProp { get; set; }
    private int PrivateField;

    public void SetAll(int publicProp, int publicFIeld, string privateSet, string privateGet, string privateProp, int privateField)
    {
        this.PublicProp = publicProp;
        this.PublicField = publicFIeld;
        this.PrivateSet = privateSet;
        this.PrivateGet = privateGet;
        this.PrivateProp = privateProp;
        this.PrivateField = privateField;
    }

    public (int, int, string?, string?, string?, int) GetAll()
    {
        return (PublicProp, PublicField, PrivateSet, PrivateGet, PrivateProp, PrivateField);
    }
}
