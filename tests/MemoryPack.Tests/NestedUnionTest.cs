namespace MemoryPack.Tests;

[MemoryPackable]
public partial record NestedUnion
{
    [MemoryPackable]
    [MemoryPackUnion(0, typeof(NestedUnionA))]
    [MemoryPackUnion(1, typeof(NestedUnionB))]
    public partial interface INestedUnion
    {

    }

    [MemoryPackable]
    public partial record NestedUnionA(string Value) : INestedUnion;

    [MemoryPackable]
    public partial record NestedUnionB(string Value) : INestedUnion;
}

[MemoryPackable]
public partial record NestedUnionContainer
{
    public required NestedUnion.INestedUnion NestedUnion { get; init; }
}

public class NestedUnionTest
{
    [Fact]
    public void CanSerializeNestedUnion()
    {
        var data = new NestedUnionContainer { NestedUnion = new NestedUnion.NestedUnionB("Foo") };
        var bytes = MemoryPackSerializer.Serialize(data);
        var result = MemoryPackSerializer.Deserialize<NestedUnionContainer>(bytes);
        result.Should().NotBeNull();
        result?.NestedUnion.Should().BeOfType<NestedUnion.NestedUnionB>();
        (result?.NestedUnion as NestedUnion.NestedUnionB)?.Value.Should().Be("Foo");
    }
}
