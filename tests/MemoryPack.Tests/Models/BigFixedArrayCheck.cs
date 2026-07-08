namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class BigFixedArrayCheck
{
    [MemoryPackArrayLength(100_000)] public byte[] BigData1;
    [MemoryPackArrayLength(1_000_000)] public byte[] BigData2;
}
