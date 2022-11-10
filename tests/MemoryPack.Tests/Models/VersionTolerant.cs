using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class VTWrapper<T>
{
    public T? Versioned { get; set; }
    public int[]? Values { get; set; }
}


[MemoryPackable(GenerateType.VersionTolerant)]
public partial class VersionTolerant0
{
}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class VersionTolerant1
{
    [MemoryPackOrder(0)]
    public int MyProperty1 { get; set; } = default;
}


[MemoryPackable(GenerateType.VersionTolerant)]
public partial class VersionTolerant2
{
    [MemoryPackOrder(0)]
    public int MyProperty1 { get; set; } = default;

    [MemoryPackOrder(1)]
    public long MyProperty2 { get; set; } = default;
}



[MemoryPackable(GenerateType.VersionTolerant)]
public partial class VersionTolerant3
{
    [MemoryPackOrder(0)]
    public int MyProperty1 { get; set; } = default;

    [MemoryPackOrder(1)]
    public long MyProperty2 { get; set; } = default;

    [MemoryPackOrder(2)]
    public short MyProperty3 { get; set; } = default;
}


[MemoryPackable(GenerateType.VersionTolerant)]
public partial class VersionTolerant4
{
    [MemoryPackOrder(0)]
    public int MyProperty1 { get; set; } = default;

    //[MemoryPackOrder(1)]
    //public long MyProperty2 { get; set; } = default;

    [MemoryPackOrder(2)]
    public short MyProperty3 { get; set; } = default;
}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class VersionTolerant5
{
    //[MemoryPackOrder(0)]
    //public int MyProperty1 { get; set; } = default;

    //[MemoryPackOrder(1)]
    //public long MyProperty2 { get; set; } = default;

    [MemoryPackOrder(2)]
    public short MyProperty3 { get; set; } = default;

    [MemoryPackOrder(5)]
    public string MyProperty6 { get; set; } = default!;
}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class Version1
{
    [MemoryPackOrder(0)]
    public int Id { get; set; }

    [MemoryPackOrder(1)]
    public string Name { get; set; } = default!;
}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class Version2
{
    [MemoryPackOrder(0)]
    public int Id { get; set; }

    //deleted
    //[MemoryPackOrder(1)] 
    //public string Name { get; set; } = default!;

    [MemoryPackOrder(2)]
    public string FirstName { get; set; } = default!;
    [MemoryPackOrder(3)]
    public string LastName { get; set; } = default!;
}
