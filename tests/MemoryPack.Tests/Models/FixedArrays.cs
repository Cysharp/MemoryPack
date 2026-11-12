namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class FixedArrayCheck
{
    [MemoryPackArrayLength(10)] public byte[] ByteData;
    [MemoryPackArrayLength(5)] public string[] StringData { get; set; }
    [MemoryPackArrayLength(1)] public Nested[] NestedData { get; set; }

    [MemoryPackable]
    public partial class Nested
    {
        public byte Data1;
        public string Data2;
        public (int, int) Data3;
    }
}


