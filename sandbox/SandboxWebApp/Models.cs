using MemoryPack;
using System.Security.Principal;

namespace SandboxWebApp;


[MemoryPackable]
[GenerateTypeScript]
public partial class AllConvertableType
{
    // supported primitives
    public bool MyBool { get; set; }
    public byte MyByte { get; set; }
    public sbyte MySByte { get; set; }
    public short MyShort { get; set; }
    public int MyInt { get; set; }
    public long MyLong { get; set; }
    public ushort MyUShort { get; set; }
    public uint MyUInt { get; set; }
    public ulong MyULong { get; set; }
    public float MyFloat { get; set; }
    public double MyDouble { get; set; }
    public Guid MyGuid { get; set; }
    public DateTime MyDate { get; set; }
    public NoMarkByteEnum MyEnum1 { get; set; }
    public NumberedUShortEnum MyEnum2 { get; set; }


    // nullable
    public bool? NullMyBool { get; set; }
    public byte? NullMyByte { get; set; }
    public sbyte? NullMySByte { get; set; }
    public short? NullMyShort { get; set; }
    public int? NullMyInt { get; set; }
    public long? NullMyLong { get; set; }
    public ushort? NullMyUShort { get; set; }
    public uint? NullMyUInt { get; set; }
    public ulong? NullMyULong { get; set; }
    public float? NullMyFloat { get; set; }
    public double? NullMyDouble { get; set; }
    public Guid? NullMyGuid { get; set; }
    public DateTime? NullMyDate { get; set; }
    public NoMarkByteEnum? NullMyEnum1 { get; set; }
    public NumberedUShortEnum? NullMyEnum2 { get; set; }

    // reference types
    public string? MyString { get; set; }
    public byte[]? MyBytes { get; set; }
    public int[]? MyIntArray { get; set; }
    public string[]? MyStringArray { get; set; }
    public List<int>? MyList { get; set; }
    public Dictionary<int, int>? MyDictionary { get; set; }
    public HashSet<int>? MySet { get; set; }
    public List<Dictionary<int, HashSet<string[]>>>? MyNestedNested { get; set; }

    public Dictionary<NoMarkByteEnum, bool>? DictCheck2 { get; set; }
    public Dictionary<Guid, int?>? DictCheck3 { get; set; }
    public Dictionary<int, string>? DictCheck4X { get; set; }

    // memory-packable
    public NestedObject? Nested1 { get; set; }
    public IMogeUnion? Union1 { get; set; }

    // not supported type
    // public LongEnumBow NonSupportLongEnumBow { get; set; }
    // public Dictionary<int, NOBOU?> ddd { get; set; }
}

public enum NOBOU : int
{
    AP = 99
}




[MemoryPackable]
[GenerateTypeScript]
public partial class NestedObject
{
    public int MyProperty { get; set; }
    public string? MyProperty2 { get; set; }
}

public enum NoMarkByteEnum : byte
{
    Apple, Orange, Grape
}

public enum NumberedUShortEnum : ushort
{
    Tokyo = 10,
    Chiba = 100,
    Saitama = 1000
}



[MemoryPackable]
[MemoryPackUnion(0, typeof(SampleUnion1))]
[MemoryPackUnion(1, typeof(SampleUnion2))]
[GenerateTypeScript]
public partial interface IMogeUnion
{
}

[MemoryPackable]
[GenerateTypeScript]
public partial class SampleUnion1 : IMogeUnion
{
    public int? MyProperty { get; set; }
}

[MemoryPackable]
[GenerateTypeScript]
public partial class SampleUnion2 : IMogeUnion
{
    public string? MyProperty { get; set; }
}


[MemoryPackable]
[GenerateTypeScript]
public partial class Subset
{
    public bool MyBool { get; set; }
    public byte MyByte { get; set; }
    public sbyte MySByte { get; set; }
    public short MyShort { get; set; }
}
