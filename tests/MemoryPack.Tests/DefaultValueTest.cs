using MemoryPack.Tests.Models;

namespace MemoryPack.Tests;

public class DefaultValueTest
{
    [Fact]
    public void SuppressDefaultInitialization()
    {
        var bin = MemoryPackSerializer.Serialize(new DefaultValuePlaceholder { X = 1 });
        var expected = new HasDefaultValue();
        var deserializedValue = MemoryPackSerializer.Deserialize<HasDefaultValue>(bin)!;
        deserializedValue.Y.Should().Be(default);
        deserializedValue.Z.Should().Be(default);
        deserializedValue.Y2.Should().Be(expected.Y2);
        deserializedValue.Z2.Should().Be(expected.Z2);
    }

    [Fact]
    public void SuppressDefaultInitialization_VersionTolerant()
    {
        var bin = MemoryPackSerializer.Serialize(new DefaultValuePlaceholderWithVersionTolerant { X = 1 });
        var expected = new HasDefaultValueWithVersionTolerant();
        var deserializedValue = MemoryPackSerializer.Deserialize<HasDefaultValueWithVersionTolerant>(bin)!;
        deserializedValue.Y.Should().Be(default);
        deserializedValue.Z.Should().Be(default);
        deserializedValue.Y2.Should().Be(expected.Y2);
        deserializedValue.Z2.Should().Be(expected.Z2);
    }
}
