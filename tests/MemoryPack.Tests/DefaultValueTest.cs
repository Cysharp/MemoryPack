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
        deserializedValue.Y.Should().Be(expected.Y);
        deserializedValue.Z.Should().Be(expected.Z);
        deserializedValue.S.Should().Be(expected.S);
        deserializedValue.B.Should().Be(expected.B);
    }

    [Fact]
    public void PropertyDefaultValue()
    {
        var bin = MemoryPackSerializer.Serialize(new DefaultValuePlaceholder { X = 1 });
        var expected = new PropertyDefaultValue();
        var deserializedValue = MemoryPackSerializer.Deserialize<PropertyDefaultValue>(bin)!;
        deserializedValue.Y.Should().Be(expected.Y);
        deserializedValue.Z.Should().Be(expected.Z);
        deserializedValue.S.Should().Be(expected.S);
        deserializedValue.B.Should().Be(expected.B);
    }

    [Fact]
    public void CtorParamDefaultValue()
    {
        var bin = MemoryPackSerializer.Serialize(new DefaultValuePlaceholder { X = 1 });
        var expected = new CtorParamDefaultValue(1);
        var deserializedValue = MemoryPackSerializer.Deserialize<CtorParamDefaultValue>(bin)!;
        deserializedValue.Y.Should().Be(expected.Y);
        deserializedValue.Z.Should().Be(expected.Z);
        deserializedValue.S.Should().Be(expected.S);
        deserializedValue.B.Should().Be(expected.B);
    }
}
