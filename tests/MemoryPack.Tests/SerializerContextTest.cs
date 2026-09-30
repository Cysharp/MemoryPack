using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System;

namespace MemoryPack.Tests;

public class SerializerContextTest
{
    [Fact]
    public void GeneratedContext_RoundTripsCompleteGraph()
    {
        var value = new ContextRoot
        {
            Id = 42,
            RuntimeType = typeof(ContextChild),
            Child = new ContextChild { Name = "nested" },
            Children = [new ContextChild { Name = "array" }],
            Matrix = new ContextChild?[,] { { new ContextChild { Name = "matrix" } } },
            Optional = new ContextValue { Name = "nullable" },
            ChildList = [new ContextChild { Name = "list" }],
            ChildMap = new Dictionary<string, ContextChild?>
            {
                ["key"] = new ContextChild { Name = "dictionary" }
            }
        };

        var context = new TestMemoryPackContext(MemoryPackSerializerOptions.Utf8);
        var bytes = MemoryPackSerializer.Serialize(value, context);
        var result = MemoryPackSerializer.Deserialize<ContextRoot, TestMemoryPackContext>(bytes, context);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
        result.RuntimeType.Should().Be(typeof(ContextChild));
        result.Child!.Name.Should().Be("nested");
        result.Children![0]!.Name.Should().Be("array");
        result.Matrix![0, 0]!.Name.Should().Be("matrix");
        result.Optional!.Value.Name.Should().Be("nullable");
        result.ChildList![0]!.Name.Should().Be("list");
        result.ChildMap!["key"]!.Name.Should().Be("dictionary");

        var nonGenericBytes = context.Serialize(typeof(ContextRoot), value);
        var nonGenericResult = context.Deserialize(typeof(ContextRoot), nonGenericBytes).Should().BeOfType<ContextRoot>().Subject;
        nonGenericResult.Id.Should().Be(42);
    }

    [Fact]
    public void GeneratedContext_SupportsBufferSequenceAndStreamApis()
    {
        var context = TestMemoryPackContext.Default;
        var value = new ContextRoot { Id = 123, Child = new ContextChild { Name = "apis" } };
        var buffer = new ArrayBufferWriter<byte>();

        MemoryPackSerializer.Serialize(buffer, value, context);
        var sequence = new ReadOnlySequence<byte>(buffer.WrittenMemory);
        var result = MemoryPackSerializer.Deserialize<ContextRoot, TestMemoryPackContext>(sequence, context);
        result!.Id.Should().Be(123);

        ContextRoot? reused = new();
        var consumed = MemoryPackSerializer.Deserialize<ContextRoot, TestMemoryPackContext>(buffer.WrittenSpan, ref reused, context);
        consumed.Should().Be(buffer.WrittenCount);
        reused!.Child!.Name.Should().Be("apis");

        using var stream = new MemoryStream();
        MemoryPackSerializer.SerializeAsync(stream, value, context).AsTask().GetAwaiter().GetResult();
        stream.Position = 0;
        var streamResult = MemoryPackSerializer.DeserializeAsync<ContextRoot, TestMemoryPackContext>(stream, context).AsTask().GetAwaiter().GetResult();
        streamResult!.Child!.Name.Should().Be("apis");
    }

    [Fact]
    public void ExistingStaticApi_RemainsAvailableForContextOwnedType()
    {
        var value = new ContextRoot
        {
            Id = 7,
            Child = new ContextChild { Name = "wire-compatible" },
            ChildList = [new ContextChild { Name = "list" }]
        };
        var context = TestMemoryPackContext.Default;

        var staticBytes = MemoryPackSerializer.Serialize(value);
        var staticResult = MemoryPackSerializer.Deserialize<ContextRoot>(staticBytes);
        var contextResult = MemoryPackSerializer.Deserialize<ContextRoot, TestMemoryPackContext>(staticBytes, context);

        var contextBytes = MemoryPackSerializer.Serialize<ContextRoot, TestMemoryPackContext>(value, context);
        var legacyResult = MemoryPackSerializer.Deserialize<ContextRoot>(contextBytes);

        staticResult!.Id.Should().Be(7);
        contextResult!.Child!.Name.Should().Be("wire-compatible");
        legacyResult!.ChildList![0]!.Name.Should().Be("list");
        contextBytes.Should().Equal(staticBytes);
    }

    [Fact]
    public void GeneratedContext_RoundTripsUnion()
    {
        var context = TestMemoryPackContext.Default;
        ContextUnion value = new ContextUnionValue { Value = 99 };

        var bytes = MemoryPackSerializer.Serialize<ContextUnion, TestMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<ContextUnion, TestMemoryPackContext>(bytes, context);

        result.Should().BeOfType<ContextUnionValue>().Which.Value.Should().Be(99);
    }

    [Fact]
    public void GeneratedContext_InitializesRecursiveFormatterGraphInTwoPhases()
    {
        var context = TestMemoryPackContext.Default;
        var value = new ContextNode { Id = 5 };
        value.Next = value;

        var bytes = MemoryPackSerializer.Serialize<ContextNode, TestMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<ContextNode, TestMemoryPackContext>(bytes, context);

        result!.Id.Should().Be(5);
        result.Next.Should().BeSameAs(result);
    }

    [Fact]
    public void GeneratedContext_RoundTripsGeneratedCollection()
    {
        var context = TestMemoryPackContext.Default;
        var value = new ContextChildCollection { new ContextChild { Name = "collection" } };

        var bytes = MemoryPackSerializer.Serialize<ContextChildCollection, TestMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<ContextChildCollection, TestMemoryPackContext>(bytes, context);

        result![0]!.Name.Should().Be("collection");
    }

    [Fact]
    public void GeneratedContext_RoundTripsClosedGenericMemoryPackable()
    {
        var context = TestMemoryPackContext.Default;
        var value = new ContextBox<ContextChild>
        {
            Value = new ContextChild { Name = "generic" }
        };

        var bytes = MemoryPackSerializer.Serialize<ContextBox<ContextChild>, TestMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<ContextBox<ContextChild>, TestMemoryPackContext>(bytes, context);
        var legacyBytes = MemoryPackSerializer.Serialize(value);
        var legacyResult = MemoryPackSerializer.Deserialize<ContextBox<ContextChild>>(legacyBytes);

        result!.Value!.Name.Should().Be("generic");
        legacyResult!.Value!.Name.Should().Be("generic");
    }

    [Fact]
    public void GeneratedContext_UsesRootFormatterOverrideForNestedDependency()
    {
        var context = TestMemoryPackContext.Default;
        var value = new ContextCustomRoot
        {
            External = new ContextExternalValue { Value = 314 }
        };

        var bytes = MemoryPackSerializer.Serialize<ContextCustomRoot, TestMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<ContextCustomRoot, TestMemoryPackContext>(bytes, context);

        result!.External!.Value.Should().Be(314);
    }

    [Fact]
    public void GeneratedContext_RoundTripsAdditionalCollectionAndTupleShapes()
    {
        var first = new ContextChild { Name = "first" };
        var second = new ContextChild { Name = "second" };
        var priority = new PriorityQueue<ContextChild?, int>();
        priority.Enqueue(first, 2);
        priority.Enqueue(second, 1);
        var lookup = new[] { first, second }.ToLookup(static x => x.Name!);
        var value = new ContextCollectionsRoot
        {
            Stack = new Stack<ContextChild?>([first, second]),
            Queue = new Queue<ContextChild?>([first, second]),
            Set = new HashSet<ContextChild?>([first, second]),
            SortedMap = new SortedDictionary<string, ContextChild?> { ["a"] = first },
            ConcurrentMap = new System.Collections.Concurrent.ConcurrentDictionary<string, ContextChild?>(new[] { new KeyValuePair<string, ContextChild?>("b", second) }),
            Sequence = new ContextChild?[] { first, second },
            Immutable = ImmutableList.Create<ContextChild?>(first, second),
            Frozen = new ContextChild?[] { first, second }.ToFrozenSet(),
            Pair = Tuple.Create<ContextChild?, string?>(first, "tuple"),
            ValuePair = (second, "value-tuple"),
            Priority = priority,
            Lookup = lookup,
        };

        var context = TestMemoryPackContext.Default;
        var bytes = MemoryPackSerializer.Serialize<ContextCollectionsRoot, TestMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<ContextCollectionsRoot, TestMemoryPackContext>(bytes, context);

        result!.Stack!.Pop()!.Name.Should().Be("second");
        result.Queue!.Dequeue()!.Name.Should().Be("first");
        result.Set.Should().HaveCount(2);
        result.SortedMap!["a"]!.Name.Should().Be("first");
        result.ConcurrentMap!["b"]!.Name.Should().Be("second");
        result.Sequence.Should().HaveCount(2);
        result.Immutable![1]!.Name.Should().Be("second");
        result.Frozen.Should().HaveCount(2);
        result.Pair!.Item2.Should().Be("tuple");
        result.ValuePair.Item1!.Name.Should().Be("second");
        result.Priority!.Dequeue()!.Name.Should().Be("second");
        result.Lookup!["first"].Single().Name.Should().Be("first");
    }
}

[MemoryPackable]
public partial class ContextRoot
{
    public int Id { get; set; }
    public Type? RuntimeType { get; set; }
    public ContextChild? Child { get; set; }
    public ContextChild?[]? Children { get; set; }
    public ContextChild?[,]? Matrix { get; set; }
    public ContextValue? Optional { get; set; }
    public List<ContextChild?>? ChildList { get; set; }
    public Dictionary<string, ContextChild?>? ChildMap { get; set; }
}

[MemoryPackable]
public partial class ContextChild
{
    public string? Name { get; set; }
}

[MemoryPackable]
public partial struct ContextValue
{
    public string? Name { get; set; }
}

[MemoryPackable]
[MemoryPackUnion(0, typeof(ContextUnionValue))]
public partial interface ContextUnion
{
}

[MemoryPackable]
public partial class ContextUnionValue : ContextUnion
{
    public int Value { get; set; }
}

[MemoryPackable(GenerateType.CircularReference)]
public partial class ContextNode
{
    [MemoryPackOrder(0)]
    public int Id { get; set; }

    [MemoryPackOrder(1)]
    public ContextNode? Next { get; set; }
}

[MemoryPackable(GenerateType.Collection)]
public partial class ContextChildCollection : List<ContextChild?>
{
}

[MemoryPackable]
public partial class ContextCollectionsRoot
{
    public Stack<ContextChild?>? Stack { get; set; }
    public Queue<ContextChild?>? Queue { get; set; }
    public HashSet<ContextChild?>? Set { get; set; }
    public SortedDictionary<string, ContextChild?>? SortedMap { get; set; }
    public System.Collections.Concurrent.ConcurrentDictionary<string, ContextChild?>? ConcurrentMap { get; set; }
    public IEnumerable<ContextChild?>? Sequence { get; set; }
    public ImmutableList<ContextChild?>? Immutable { get; set; }
    public FrozenSet<ContextChild?>? Frozen { get; set; }
    public Tuple<ContextChild?, string?>? Pair { get; set; }
    public (ContextChild?, string?) ValuePair { get; set; }
    public PriorityQueue<ContextChild?, int>? Priority { get; set; }
    public ILookup<string, ContextChild>? Lookup { get; set; }
}

[MemoryPackable]
public partial class ContextBox<T>
{
    public T? Value { get; set; }
}

[MemoryPackable]
public partial class ContextCustomRoot
{
    [MemoryPackAllowSerialize]
    public ContextExternalValue? External { get; set; }
}

public sealed class ContextExternalValue
{
    public int Value { get; set; }
}

public sealed class ContextExternalValueFormatter : MemoryPackFormatter<ContextExternalValue>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ContextExternalValue? value)
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteUnmanaged(value.Value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ContextExternalValue? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }
        if (count != 1)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);
        }

        reader.ReadUnmanaged(out int item);
        value = new ContextExternalValue { Value = item };
    }
}

[MemoryPackSerializable(typeof(ContextRoot))]
[MemoryPackSerializable(typeof(ContextRoot[]))]
[MemoryPackSerializable(typeof(List<ContextRoot>))]
[MemoryPackSerializable(typeof(Dictionary<ContextRoot, ContextChild>))]
[MemoryPackSerializable(typeof(ContextUnion))]
[MemoryPackSerializable(typeof(ContextNode))]
[MemoryPackSerializable(typeof(ContextChildCollection))]
[MemoryPackSerializable(typeof(ContextCollectionsRoot))]
[MemoryPackSerializable(typeof(ContextBox<ContextChild>))]
[MemoryPackSerializable(typeof(ContextCustomRoot))]
[MemoryPackSerializable(typeof(ContextExternalValue), FormatterType = typeof(ContextExternalValueFormatter))]
public partial class TestMemoryPackContext : MemoryPackSerializerContext
{
}
