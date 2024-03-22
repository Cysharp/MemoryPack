using MemoryPack.Tests.Models;

namespace MemoryPack.Tests;

public class DefaultValueTest
{
    [Fact]
    public void FieldDefaultValue()
    {
        var bin = MemoryPackSerializer.Serialize(new DefaultValuePlaceholder { X = 1 });
        var expected = new FieldDefaultValue();
        var deserializedValue = MemoryPackSerializer.Deserialize<FieldDefaultValue>(bin)!;
        deserializedValue.Y.Should().Be(default);
        deserializedValue.Z.Should().Be(expected.Z);
        deserializedValue.FromMethod.Should().Be(expected.FromMethod);
    }

    [Fact]
    public void PropertyDefaultValue()
    {
        var bin = MemoryPackSerializer.Serialize(new DefaultValuePlaceholder { X = 1 });
        var expected = new PropertyDefaultValue();
        var deserializedValue = MemoryPackSerializer.Deserialize<PropertyDefaultValue>(bin)!;
        deserializedValue.Y.Should().Be(default);
        deserializedValue.Z.Should().Be(expected.Z);
        deserializedValue.FromMethod.Should().Be(expected.FromMethod);
    }
}
