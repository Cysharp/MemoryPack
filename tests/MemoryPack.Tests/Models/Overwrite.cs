using System;

namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class Overwrite
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
    public String? MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }
}

[MemoryPackable]
public partial struct Overwrite2
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
    public String? MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }
}


[MemoryPackable]
public partial class Overwrite3
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
    public String? MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }

    public Overwrite3(int myProperty1, int myProperty2)
    {
        this.MyProperty1 = myProperty1;
        this.MyProperty2 = myProperty2;
    }
}

[MemoryPackable]
public partial class Overwrite4
{
    public int MyProperty1 { get; set; }
    public Overwrite? MyProperty2 { get; set; }
}



// TODO: test List, Array, etc...
