namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class Recursive
{
    public int MyProperty { get; set; }
    public Recursive? Rec { get; set; }
}
