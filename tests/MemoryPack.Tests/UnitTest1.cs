namespace MemoryPack.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var span = new byte[] { 1, 10, 100, 255 };
        var reader = new MemoryPackReader(span);

        ref var spanRef = ref reader.GetSpanReference(5);
    }
}
