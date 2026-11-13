#if NET11_0_OR_GREATER
using System;
using System.Linq;

namespace MemoryPack.Tests;

[MemoryPackable]
public partial record CSharpUnionCat(string Name);

[MemoryPackable]
public partial record CSharpUnionDog(string Name, int Age);

[MemoryPackable]
public partial union CSharpUnionPet(CSharpUnionCat, CSharpUnionDog, int?, string);

[MemoryPackable]
public partial union CSharpUnionResult<T>(T, Exception);

[MemoryPackable]
public partial class CSharpUnionHolder
{
    public CSharpUnionPet Pet { get; set; }
    public CSharpUnionPet[]? Pets { get; set; }
    public CSharpUnionPet? MaybePet { get; set; }
}

[MemoryPackable]
public partial union CSharpUnionV1(int, string);

[MemoryPackable]
public partial union CSharpUnionV2(int, string, CSharpUnionCat);

[MemoryPackable]
public partial union CSharpUnionWithBody(int, string)
{
    public static int SerializingCount;
    public static int DeserializedCount;

    public CSharpUnionWithBody() : this(1) { }

    public CSharpUnionWithBody(int a, int b) : this(a + b) { }

    public string Description => Value?.ToString() ?? "";

    [MemoryPackOnSerializing]
    static void OnSerializing() => SerializingCount++;

    [MemoryPackOnDeserialized]
    static void OnDeserialized() => DeserializedCount++;
}

public partial class CSharpUnionContainer
{
    [MemoryPackable]
    public partial union Nested(int, string);
}

[MemoryPackable]
public partial union CSharpUnionOuter(CSharpUnionOuter.Inner, int)
{
    [MemoryPackable]
    public partial record Inner(string Name);
}

[MemoryPackable]
public partial union CSharpUnionOfUnion(CSharpUnionPet, long);

[MemoryPackable]
public partial record CSharpUnionBase(string Name);

[MemoryPackable]
public partial record CSharpUnionDerived(string Name, int Age) : CSharpUnionBase(Name);

// derived case first, as the first matching case is used
[MemoryPackable]
public partial union CSharpUnionOverlap(CSharpUnionDerived, CSharpUnionBase);

[MemoryPackable]
[MemoryPackUnion(10, typeof(int?))]
[MemoryPackUnion(20, typeof(string))]
public partial union CSharpUnionTagged(int?, string);

// same tags as CSharpUnionTagged, cases reordered
[MemoryPackable]
[MemoryPackUnion(20, typeof(string))]
[MemoryPackUnion(10, typeof(int?))]
public partial union CSharpUnionTaggedReordered(string, int?);

[MemoryPackable]
public partial union CSharpUnionTuple((int, string), long);

public class CSharpUnionTest
{
    static CSharpUnionPet RoundTrip(CSharpUnionPet value)
    {
        return MemoryPackSerializer.Deserialize<CSharpUnionPet>(MemoryPackSerializer.Serialize(value));
    }

    [Fact]
    public void Cases()
    {
        RoundTrip(new CSharpUnionCat("Tom")).Value.Should().Be(new CSharpUnionCat("Tom"));
        RoundTrip(new CSharpUnionDog("Rex", 3)).Value.Should().Be(new CSharpUnionDog("Rex", 3));
        RoundTrip((int?)42).Value.Should().Be(42);
        RoundTrip("foo").Value.Should().Be("foo");
    }

    [Fact]
    public void Null()
    {
        RoundTrip(default).Value.Should().BeNull();
        RoundTrip((int?)null).Value.Should().BeNull();
    }

    [Fact]
    public void Tag()
    {
        // union header then value
        MemoryPackSerializer.Serialize<CSharpUnionPet>(new CSharpUnionDog("Rex", 3))[0].Should().Be(1);
        MemoryPackSerializer.Serialize<CSharpUnionPet>(default).Should().Equal(MemoryPackCode.NullObject);
    }

    [Fact]
    public void Generic()
    {
        var bin = MemoryPackSerializer.Serialize<CSharpUnionResult<int>>(10);
        MemoryPackSerializer.Deserialize<CSharpUnionResult<int>>(bin).Value.Should().Be(10);
    }

    [Fact]
    public void Member()
    {
        var holder = new CSharpUnionHolder { Pet = new CSharpUnionCat("Tom"), Pets = [new CSharpUnionDog("Rex", 3), default, "foo"] };
        var result = MemoryPackSerializer.Deserialize<CSharpUnionHolder>(MemoryPackSerializer.Serialize(holder))!;
        result.Pet.Value.Should().Be(new CSharpUnionCat("Tom"));
        result.Pets!.Select(x => x.Value).Should().Equal(new CSharpUnionDog("Rex", 3), null, "foo");
        result.MaybePet.Should().BeNull();

        holder.MaybePet = "bar";
        MemoryPackSerializer.Deserialize<CSharpUnionHolder>(MemoryPackSerializer.Serialize(holder))!.MaybePet!.Value.Value.Should().Be("bar");
    }

    [Fact]
    public void AddedCase()
    {
        var bin = MemoryPackSerializer.Serialize<CSharpUnionV1>("foo");
        MemoryPackSerializer.Deserialize<CSharpUnionV2>(bin).Value.Should().Be("foo");
    }

    [Fact]
    public void UnknownTag()
    {
        var bin = MemoryPackSerializer.Serialize<CSharpUnionV2>(new CSharpUnionCat("Tom"));
        var action = () => MemoryPackSerializer.Deserialize<CSharpUnionV1>(bin);
        action.Should().Throw<MemoryPackSerializationException>();
    }

    [Fact]
    public void Body()
    {
        var serializing = CSharpUnionWithBody.SerializingCount;
        var deserialized = CSharpUnionWithBody.DeserializedCount;

        var bin = MemoryPackSerializer.Serialize<CSharpUnionWithBody>("foo");
        bin.Should().Equal(MemoryPackSerializer.Serialize<CSharpUnionV1>("foo"));
        MemoryPackSerializer.Deserialize<CSharpUnionWithBody>(bin).Value.Should().Be("foo");

        CSharpUnionWithBody.SerializingCount.Should().Be(serializing + 1);
        CSharpUnionWithBody.DeserializedCount.Should().Be(deserialized + 1);
    }

    [Fact]
    public void Nested()
    {
        var bin = MemoryPackSerializer.Serialize<CSharpUnionContainer.Nested>("foo");
        MemoryPackSerializer.Deserialize<CSharpUnionContainer.Nested>(bin).Value.Should().Be("foo");

        var bin2 = MemoryPackSerializer.Serialize<CSharpUnionOuter>(new CSharpUnionOuter.Inner("Tom"));
        MemoryPackSerializer.Deserialize<CSharpUnionOuter>(bin2).Value.Should().Be(new CSharpUnionOuter.Inner("Tom"));
    }

    [Fact]
    public void UnionCase()
    {
        var bin = MemoryPackSerializer.Serialize<CSharpUnionOfUnion>((CSharpUnionPet)new CSharpUnionCat("Tom"));
        var result = MemoryPackSerializer.Deserialize<CSharpUnionOfUnion>(bin).Value;
        result.Should().BeOfType<CSharpUnionPet>().Which.Value.Should().Be(new CSharpUnionCat("Tom"));
    }

    [Fact]
    public void OverlappingCases()
    {
        var derived = MemoryPackSerializer.Serialize<CSharpUnionOverlap>(new CSharpUnionDerived("Rex", 3));
        derived[0].Should().Be(0);
        MemoryPackSerializer.Deserialize<CSharpUnionOverlap>(derived).Value.Should().Be(new CSharpUnionDerived("Rex", 3));

        var baseValue = MemoryPackSerializer.Serialize<CSharpUnionOverlap>(new CSharpUnionBase("Tom"));
        baseValue[0].Should().Be(1);
        MemoryPackSerializer.Deserialize<CSharpUnionOverlap>(baseValue).Value.Should().Be(new CSharpUnionBase("Tom"));
    }

    [Fact]
    public void ExplicitTag()
    {
        var bin = MemoryPackSerializer.Serialize<CSharpUnionTagged>("foo");
        bin[0].Should().Be(20);
        MemoryPackSerializer.Deserialize<CSharpUnionTagged>(bin).Value.Should().Be("foo");

        var bin2 = MemoryPackSerializer.Serialize<CSharpUnionTagged>((int?)42);
        bin2[0].Should().Be(10);
        MemoryPackSerializer.Deserialize<CSharpUnionTagged>(bin2).Value.Should().Be(42);
    }

    [Fact]
    public void ExplicitTagReordered()
    {
        var bin = MemoryPackSerializer.Serialize<CSharpUnionTagged>("foo");
        MemoryPackSerializer.Deserialize<CSharpUnionTaggedReordered>(bin).Value.Should().Be("foo");

        var bin2 = MemoryPackSerializer.Serialize<CSharpUnionTagged>((int?)42);
        MemoryPackSerializer.Deserialize<CSharpUnionTaggedReordered>(bin2).Value.Should().Be(42);
    }

    [Fact]
    public void TupleCase()
    {
        var bin = MemoryPackSerializer.Serialize<CSharpUnionTuple>((1, "foo"));
        MemoryPackSerializer.Deserialize<CSharpUnionTuple>(bin).Value.Should().Be((1, "foo"));
    }
}
#endif
