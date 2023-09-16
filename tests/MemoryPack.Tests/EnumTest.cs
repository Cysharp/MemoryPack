namespace MemoryPack.Tests;

public class EnumTest
{
    private T Convert<T>(T value)
    {
        return MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value))!;
    }

    [Fact]
    public void EnumTes()
    {
        Convert(BEnum.B).Should().Be(BEnum.B);
        Convert(NormalEnum.A).Should().Be(NormalEnum.A);
        Convert(NotNotEnum.C).Should().Be(NotNotEnum.C);
    }

    public enum BEnum : byte
    {
        A, B, C
    }
    public enum NormalEnum
    {
        A, B, C
    }

    public enum NotNotEnum : long
    {
        A, B, C
    }
}
