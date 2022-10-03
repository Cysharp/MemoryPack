# MemoryPack
[![GitHub Actions](https://github.com/Cysharp/MemoryPack/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/MemoryPack/actions) [![Releases](https://img.shields.io/github/release/Cysharp/MemoryPack.svg)](https://github.com/Cysharp/MemoryPack/releases)

Zero encoding extreme performance binary serializer for C#.

![image](https://user-images.githubusercontent.com/46207/192748136-262ac2e7-4646-46e1-afb8-528a51a4a987.png)

For standard object, MemoryPack is x3 faster than MessagePack for C#. For struct array, MemoryPack gots boosted power, x50~100 faster than other serializers.

MemoryPack is my 4th serializer, previously I've created well known serializers, ~~[ZeroFormatter](https://github.com/neuecc/ZeroFormatter)~~, ~~[Utf8Json](https://github.com/neuecc/Utf8Json)~~, [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp). The reason for MemoryPack's speed is due to its C#-specific, C#-optimized binary format and a well tuned implementation based on my past experience. It is also a completely new design utilizing .NET 7 and C# 11 and the Incremental Source Generator.

Other serializers performs many encoding operations such as VarInt encoding, tag, UTF8 Encoding, etc. MemoryPack format uses a zero-encoding design that copies as much of the C# memory as possible. zero-encoding is similar as FlatBuffers but don't need special type, MemoryPack's serialize target is POCO.

Other than performance, MemoryPack has these features.

* Support modern I/O APIs(`IBufferWriter<byte>`, `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>`)
* Native AOT friendly Source Generator based code generation, no Dynamic CodeGen(IL.Emit)
* Reflectionless non-generics APIs
* Deserialize into existing instance
* Polymorphism(Union) serialization
* PipeWriter/Reader based streaming serialization

Installation
---
This library is distributed via NuGet. Minimum requirement is `.NET 7 RC1`.

> PM> Install-Package [MemoryPack](https://www.nuget.org/packages/MemoryPack)

And you need to enable preview features to `.csproj`.

```xml
<PropertyGroup>
    <EnablePreviewFeatures>True</EnablePreviewFeatures>
</PropertyGroup>
```

Quick Start
---
Define the struct or class to be serialized and annotate it with a `[MemoryPackable]` attribute and `partial` keyword.

```csharp
using MemoryPack;

[MemoryPackable]
public partial class Person
{
    public int Age { get; set; }
    public string Name { get; set; }
}
```

Serialization code is generated via C# source generator feature, that implements `IMemoryPackable<T>` interface. In Visual Studio, you can check generated code via `Ctrl+K, R` on class name and select `*.MemoryPackFormatter.g.cs`.

Call `MemoryPackSerializer.Serialize<T>/Deserialize<T>` to serialize/deserialize your object instance.

```csharp
var v = new Person { Age = 40, Name = "John" };

var bin = MemoryPackSerializer.Serialize(v);
var val = MemoryPackSerializer.Deserialize<Person>(bin);
```

Serialize method supports return `byte[]` and serialize to `IBufferWriter<byte>` or `Stream`. Deserialize method supports `ReadOnlySpan<byte>`, `ReadOnlySeqeunce<byte>` and `Stream`. And also there have non-generics version.

Built-in supported types
---
These types can serialize by default:

* .NET primitives (`byte`, `int`, `bool`, `char`, `double`, etc...)
* Unmanaged types(Any `enum`, Any user-defined `strcut` that no contains reference type)
* `string`, `decimal`, `Half`, `Int128`, `UInt128`, `Guid`, `Rune`, `BigInteger`
* `TimeSpan`,  `DateTime`, `DateTimeOffset`, `TimeOnly`, `DateOnly`, `TimeZoneInfo`
* `Complex`, `Plane`, `Quaternion` `Matrix3x2`, `Matrix4x4`, `Vector2`, `Vector3`, `Vector4`
* `Uri`, `Version`, `StringBuilder`, `Type`, `BitArray`
* `T[]`, `T[,]`, `T[,,]`, `T[,,,]`, `Memory<>`, `ReadOnlyMemory<>`, `ArraySegment<>`, `ReadOnlySequence<>`
* `Nullable<>`, `Lazy<>`, `KeyValuePair<,>`, `Tuple<,...>`, `ValueTuple<,...>`
* `List<>`, `LinkedList<>`, `Queue<>`, `Stack<>`, `HashSet<>`, `PriorityQueue<,>`
* `Dictionary<,>`, `SortedList<,>`, `SortedDictionary<,>`,  `ReadOnlyDictionary<,>` 
* `Collection<>`, `ReadOnlyCollection<>`,`ObservableCollection<>`, `ReadOnlyObservableCollection<>`
* `IEnumerable<>`, `ICollection<>`, `IList<>`, `IReadOnlyCollection<>`, `IReadOnlyList<>`, `ISet<>`
* `IDictionary<,>`, `IReadOnlyDictionary<,>`, `ILookup<,>`, `IGrouping<,>`,
* `ConcurrentBag<>`, `ConcurrentQueue<>`, `ConcurrentStack<>`, `ConcurrentDictionary<,>`, `BlockingCollection<>`
* Immutable collections (`ImmutableList<>`, etc) and interfaces (`IImmutableList<>`, etc)

Define `[MemoryPackable]` `class` / `struct` / `record` / `record struct`
---
`[MemoryPackable]` can annotate to any `class`, `struct`, `record`, `record struct` and `interface`. If type is `struct` or `record struct` and that contains no reference type([C# Unmanaged types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types)), any additional annotation(ignore, include, constructor, callbacks) is not used, that serialize/deserialize directly from the memory.

Otherwise, in the default, `[MemoryPackable]` serializes public instance property or field. You can use `[MemoryPackIgnore]` to remove serialization target, `[MemoryPackInclude]` promotes a private member to serialization target.

```csharp
[MemoryPackable]
public partial class Sample
{
    // these types are serialized by default
    public int PublicField;
    public readonly int PublicReadOnlyField;
    public int PublicProperty { get; set; }
    public int PrivateSetPublicProperty { get; private set; }
    public int ReadOnlyPublicProperty { get; }
    public int InitProperty { get; init; }
    public required int RequiredInitProperty { get; init; }

    // these types are not serialized by default
    int privateProperty { get; set; }
    int privateField;
    readonly int privateReadOnlyField;

    // use [MemoryPackIgnore] to remove target of public member
    [MemoryPackIgnore]
    public int PublicProperty2 => PublicProperty + PublicField;

    // use [MemoryPackInclude] to promote private member to serialization target
    [MemoryPackInclude]
    int privateField2;
    [MemoryPackInclude]
    int privateProperty2 { get; set; }
}
```

Which members are serialized, you can check IntelliSense in type(code genreator makes serialization info to `<remarks />` comment).

![image](https://user-images.githubusercontent.com/46207/192393984-9af01fcb-872e-46fb-b08f-4783e8cef4ae.png)

All members must be memorypack-serializable, if not, code generator reports error.

![image](https://user-images.githubusercontent.com/46207/192413557-8a47d668-5339-46c5-a3da-a77841666f81.png)

MemoryPack has 24 diagnostics rules(`MEMPACK001` to `MEMPACK026`) to be define comfortably.

If target type is defined MemoryPack serialization externally and registered, use `[MemoryPackAllowSerialize]` to silent diagnostics.

```csharp
[MemoryPackable]
public partial class Sample2
{
    [MemoryPackAllowSerialize]
    public NotSerializableType? NotSerializableProperty { get; set; }
}
```

Member order is **important**, MemoryPack does not serialize any member-name and other tags, serialize in the declared order. If the type is inherited, serialize in the order of parent → child. Member orders can not change for the deserialization. For the schema evolution, see [Version tolerant](#version-tolerant)  section.

Default order is sequential but you can choose explicit layout with `[MemoryPackable(SerializeLayout.Explicit)]` and `[MemoryPackOrder()]`.

```csharp
// serialize Prop0 -> Prop1
[MemoryPackable(SerializeLayout.Explicit)]
public partial class SampleExplicitOrder
{
    [MemoryPackOrder(1)]
    public int Prop1 { get; set; }
    [MemoryPackOrder(0)]
    public int Prop0 { get; set; }
}
```

### Constructor selection

MemoryPack supports parameterized constructor not only parameterless constructor. The selection of the constructor follows these rules. Both class and struct follows same.

* If has `[MemoryPackConstructor]`, use it
* If has no explicit constructor(includes private), use parameterless one
* If has a one parameterless/parameterized constructor(includes private), use it
* If has multiple constructors, must apply `[MemoryPackConstructor]` attribute(no automatically choose one), otherwise generator error it.
* If choosed parameterized constructor, all parameter name must match with member name(case-insensitive)

```csharp
[MemoryPackable]
public partial class Person
{
    public readonly int Age;
    public readonly string Name;

    // You can use parametarized constructor(paramter name must match with member names)
    public Person(int age, string name)
    {
        this.Age = age;
        this.Name = name;
    }
}

// also supports record primary constructor
[MemoryPackable]
public partial record Person2(int Age, string Name);

public partial class Person3
{
    public int Age { get; set; }
    public string Name { get; set; }

    public Person3()
    {
    }

    // If exists multiple constructors, must use [MemoryPackConstructor]
    [MemoryPackConstructor]
    public Person3(int age, string name)
    {
        this.Age = age;
        this.Name = name;
    }
}
```

### Serialization callbacks

When serialize, deserialize, MemoryPack can hook before/after event with `[MemoryPackOnSerializing]`, `[MemoryPackOnSerialized]`, `[MemoryPackOnDeserializing]`, `[MemoryPackOnDeserialized]` attributes. It can annotate both static and instance, public and private method but must be paramterless method.

```csharp
[MemoryPackable]
public partial class MethodCallSample
{
    // method call order is static -> instance
    [MemoryPackOnSerializing]
    public static void OnSerializing1()
    {
        Console.WriteLine(nameof(OnSerializing1));
    }

    // also allows private method
    [MemoryPackOnSerializing]
    void OnSerializing2()
    {
        Console.WriteLine(nameof(OnSerializing2));
    }

    // serializing -> /* serialize */ -> serialized
    [MemoryPackOnSerialized]
    static void OnSerialized1()
    {
        Console.WriteLine(nameof(OnSerialized1));
    }

    [MemoryPackOnSerialized]
    public void OnSerialized2()
    {
        Console.WriteLine(nameof(OnSerialized2));
    }

    [MemoryPackOnDeserializing]
    public static void OnDeserializing1()
    {
        Console.WriteLine(nameof(OnDeserializing1));
    }

    // Note: instance method with MemoryPackOnDeserializing, that not called if instance is not passed by `ref`
    [MemoryPackOnDeserializing]
    public void OnDeserializing2()
    {
        Console.WriteLine(nameof(OnDeserializing2));
    }

    [MemoryPackOnDeserialized]
    public static void OnDeserialized1()
    {
        Console.WriteLine(nameof(OnDeserialized1));
    }

    [MemoryPackOnDeserialized]
    public void OnDeserialized2()
    {
        Console.WriteLine(nameof(OnDeserialized2));
    }
}
```

Define custom collection
---
In default, annotated `[MemoryPackObject]` type try to search members. However if type is collection(`ICollection<>`, `ISet<>`, `IDictionary<,>`), you can change `GenreateType.Collection` to serialize correctly.

```csharp
[MemoryPackable(GenerateType.Collection)]
public partial class MyList<T> : List<T>
{
}

[MemoryPackable(GenerateType.Collection)]
public partial class MyStringDictionary<TValue> : Dictionary<string, TValue>
{

}
```

Polymorphism(Union)
---
MemoryPack supports serializing interface and abstract class objects for polymorphism serialization. In MemoryPack these are called Union. Only interfaces and abstracts classes are allowed to be annotated with `[MemoryPackUnion]` attributes. Unique union tags are required.

```csharp
// Annotate [MemoryPackable] and inheritance types by [MemoryPackUnion]
// Union also supports abstract class
[MemoryPackable]
[MemoryPackUnion(0, typeof(FooClass))]
[MemoryPackUnion(1, typeof(BarClass))]
public partial interface IUnionSample
{
}

[MemoryPackable]
public partial class FooClass : IUnionSample
{
    public int XYZ { get; set; }
}

[MemoryPackable]
public partial class BarClass : IUnionSample
{
    public string? OPQ { get; set; }
}
// ---

IUnionSample data = new FooClass() { XYZ = 999 };

// Serialize as interface type.
var bin = MemoryPackSerializer.Serialize(data);

// Deserialize as interface type.
var reData = MemoryPackSerializer.Deserialize<IUnionSample>(bin);

switch (reData)
{
    case FooClass x:
        Console.WriteLine(x.XYZ);
        break;
    case BarClass x:
        Console.WriteLine(x.OPQ);
        break;
    default:
        break;
}
```

Serialize API
---
Serialize has three overloads.

```csharp
// Non generic API also available, these version is first argument is Type and value is object?
byte[] Serialize<T>(in T? value, MemoryPackSerializeOptions? options = default)
void Serialize<T, TBufferWriter>(in TBufferWriter bufferWriter, in T? value, MemoryPackSerializeOptions? options = default)
async ValueTask SerializeAsync<T>(Stream stream, T? value, MemoryPackSerializeOptions? options = default, CancellationToken cancellationToken = default)
```

The recommended way to do this in Performance is to use `BufferWriter`. This serializes directly into the buffer. It can be applied to `PipeWriter` in `System.IO.Pipelines`, `BodyWriter` in ASP .NET Core, etc.

If a `byte[]` is required (e.g. `RedisValue` in [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)), return `byte[]` API is simple and almostly fast.

Note that `SerializeAsync` for `Stream` is asynchronous only for Flush; it serializes everything once into MemoryPack's internal pool buffer and then writes it out with WriteAsync. Therefore, BufferWriter overloading, which separates and controls buffer and flush, is better.

If you want to do complete streaming write, see [Streaming Serialization](#streaming-serialization) section.

### MemoryPackSerializeOptions

`MemoryPackSerializeOptions` configures how serialize string as Utf16 or Utf8. If passing null then uses `MemoryPackSerializeOptions.Default`, it is same as `MemoryPackSerializeOptions.Utf8`, in other words, serialize the string as Utf8. If you want to serialize with Utf16, you can use `MemoryPackSerializeOptions.Utf16`.

Since C#'s internal string representation is UTF16, UTF16 performs better. However, the payload tends to be larger; in UTF8, an ASCII string is one byte, while in UTF16 it is two bytes. Because the difference in size of this payload is so large, UTF8 is set by default.

If the data is non-ASCII (e.g. Japanese, which can be more than 3 bytes, and UTF8 is larger), or if you have to compress it separately, UTF16 may give better results.

Whether UTF8 or UTF16 is selected during serialization, it is not necessary to specify it during deserialization. It will be automatically detected and deserialized normally.

Deserialize API
---
Deserialize has `ReadOnlySpan<byte>` and `ReadOnlySequence<byte>`, `Stream` overload and `ref` support.

```csharp
T? Deserialize<T>(ReadOnlySpan<byte> buffer)
void Deserialize<T>(ReadOnlySpan<byte> buffer, ref T? value)
T? Deserialize<T>(in ReadOnlySequence<byte> buffer)
void Deserialize<T>(in ReadOnlySequence<byte> buffer, ref T? value)
async ValueTask<T?> DeserializeAsync<T>(Stream stream)
```

`ref` overload overwrite existing instance, for details see [Overwrite](#overwrite) section.

`DeserializeAsync(Stream)` is not completely streaming read, first read into MemoryPack's internal pool up to the end-of-stream, then deserialize.

If you want to do complete streaming read, see [Streaming Serialization](#streaming-serialization) section.

Overwrite
---
MemoryPack supports deserialize to existing instance, that reduce new instance allocation. It can use by `Deserialize(ref T? value)` overload.

```csharp
var person = new Person();
var bin = MemoryPackSerializer.Serialize(person);

// overwrite data to existing instance.
MemoryPackSerializer.Deserialize(bin, ref person);
```

MemoryPack will attempt to overwrite as much as possible, but if the conditions do not match, it will create a new instance (as in normal deserialization).

* ref value(includes members in object graph) is null, set new instance
* only allows parameterless constructor, if parametarized constructor is used, create new instance
* if value is `T[]`, reuse only if the length is the same, otherwise create new instance
* if value is collection that has `.Clear()` method(`List<>`, `Stack<>`, `Queue<>`, `LinkedList<>`, `HashSet<>`, `PriorityQueue<,>`, `ObservableCollection`, `Collection`, `ConcurrentQueue<>`, `ConcurrentStack<>`, `ConcurrentBag<>`, `Dictionary<,>`, `SoretedDictionary<,>`, `SortedList<,>`, `ConcurrentDictionary<,>`) call Clear() and reuse it, otherwise create new instance

Version tolerant
---
MemoryPack supports schema evolution limitedly.

* unmanaged struct can't change any more
* MemoryPackable objects, members can be added, but not deleted
* MemoryPackable objects, can't change member order

```csharp
[MemoryPackable]
public partial class VersionCheck
{
    public int Prop1 { get; set; }
    public long Prop2 { get; set; }
}

// Add is OK.
[MemoryPackable]
public partial class VersionCheck
{
    public int Prop1 { get; set; }
    public long Prop2 { get; set; }
    public int? AddedProp { get; set; }
}

// Remove is NG.
[MemoryPackable]
public partial class VersionCheck
{
    // public int Prop1 { get; set; }
    public long Prop2 { get; set; }
}

// Change order is NG.
[MemoryPackable]
public partial class VersionCheck
{
    public long Prop2 { get; set; }
    public int Prop1 { get; set; }
}
```

Next [Serialization info](#serialization-info) section shows how to check for schema changes, e.g., by CI, to prevent accidents.

Serialization info
----
Which members are serialized, you can check IntelliSense in type. There is an option to write that information to a file at compile time. Set `MemoryPackGenerator_SerializationInfoOutputDirectory` as follows.

```xml
<!-- output memoerypack serialization info to directory -->
<ItemGroup>
    <CompilerVisibleProperty Include="MemoryPackGenerator_SerializationInfoOutputDirectory" />
</ItemGroup>
<PropertyGroup>
    <MemoryPackGenerator_SerializationInfoOutputDirectory>$(MSBuildProjectDirectory)\MemoryPackLogs</MemoryPackGenerator_SerializationInfoOutputDirectory>
</PropertyGroup>
```

The following info is written to the file.

![image](https://user-images.githubusercontent.com/46207/192460684-c2fd8bcb-375e-41dd-9960-58205d5b1b7a.png)

If the type is unmanaged, showed `unmanaged` before type name.

```txt
unmanaged FooStruct
---
int x
int y
```

By checking the differences in this file, dangerous schema changes can be prevented. For example, you may want to use CI to detect the following rules

* modify unmanaged type
* member order change
* member deletion

Performance
---
TODO for describe details, stay tuned.

Payload size and compression
---
Payload size depends on the target value; unlike JSON, there are no keys and it is a binary format, so the payload size is likely to be smaller than JSON.

For those with varint encoding, such as MessagePack and Protobuf, MemoryPack tends to be larger if ints are used a lot (in MemoryPack, ints are always 4 bytes due to fixed size encoding, while MsgPack is 1~5 bytes).

float and double are 4 bytes and 8 bytes in MemoryPack, but 5 bytes and 9 bytes in MsgPack. So MemoryPack is smaller, for example, for Vector3 (float, float, float) arrays.

String is UTF8 by default, which is similar to other serializers, but if the UTF16 option is chosen, it will be of a different nature.

In any case, if the payload size is large, compression should be considered. LZ4, ZStandard and Brotli are recommended. An efficient way to combine compression and serialization will be presented at a later date.

Packages
---
MemoryPack has four packages.

* MemoryPack
* MemoryPack.Core
* MemoryPack.Generator
* MemoryPack.Streaming

Mainly you only reference `MemoryPack`, this both `MemoryPack.Core` and `MemoryPack.Generator`. If you want to use [Streaming Serialization](#streaming-serialization), additionaly use `MemoryPack.Streaming`.

Streaming Serialization
---
`MemoryPack.Streaming` provides additional `MemoryPackStreamingSerializer`, it serialize/deserialize collection data streamingly.

```csharp
public static class MemoryPackStreamingSerializer
{
    public static async ValueTask SerializeAsync<T>(PipeWriter pipeWriter, int count, IEnumerable<T> source, int flushRate = 4096, CancellationToken cancellationToken = default)
    public static async ValueTask SerializeAsync<T>(Stream stream, int count, IEnumerable<T> source, int flushRate = 4096, CancellationToken cancellationToken = default)
    public static async IAsyncEnumerable<T?> DeserializeAsync<T>(PipeReader pipeReader, int bufferAtLeast = 4096, int readMinimumSize = 8192, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    public static IAsyncEnumerable<T?> DeserializeAsync<T>(Stream stream, int bufferAtLeast = 4096, int readMinimumSize = 8192, CancellationToken cancellationToken = default)
}
```

Formatter/Provider API
---
If you want to implement formatter manually, inherit `MemoryPackFormatter<T>` to recommended.

```csharp
public class SkeltonFormatter : MemoryPackFormatter<Skelton>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Skelton? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        // use writer method.
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Skelton? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        // use reader method.
    }
}
```
The created formatter is registered with `MemoryPackFormatterProvider`.

```csharp
MemoryPackFormatterProvider.Register(new SkeltonFormatter());
```

Unity support
---
Currently MemoryPack dependents .NET 7 runtime, incremental generator and C# 11. Therefore it will not work in Unity. .NET 7 support is planned for Unity 2025.

If you request it, there is a possibility to make a detuned Unity version. Please send your request to Issues with the case you wish to use.

Binary wire format specification
---
The type of `T` defined in `Serialize<T>` and `Deserialize<T>` is called C# schema. MemoryPack format is not self described format. Deserialize requires the corresponding C# schema. Five types exist as internal representations of binaries, but types cannot be determined without a C# schema.

There are no endian specifications. It is not possible to convert on machines with different endianness. However modern computers are usually little-endian.

There are five value types of format.

* Unmanaged struct
* Object
* Collection
* String
* Union

### Unmanaged struct

Unmanaged struct is C# struct that no contains reference type, similar constraint of [C# Unmanaged types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types). Serializing struct layout as is, includes padding.

### Object

`{byte memberCount, values...}`

Object has 1byte unsigned byte as member count in header. Member count allows `0` to `249`, `255` represents object is `null`. Values store memorypack value for the number of member count.


### Collection

`[int length, values...]`

Collection has 4byte signed interger as data count in header, `-1` represents `null`. Values store memorypack value for the number of length.

### String

`(int utf16-length, utf16-value)`  
`(int ~utf8-length, int utf16-length, utf8-value)`

String has two-form, UTF16 and UTF8. If first 4byte signed integer is `-1`, represents null. `0`, represents empty. UTF16 is same as collection(serialize as `ReadOnlySpan<char>`, utf16-value's byte count is utf16-length * 2). If first signed integer <= `-2`, value is encoded by UTF8. utf8-length is encoded in complement, `~utf8-length` to retrieve length. Next signed integer is utf16-length, it allows `-1` that represents unknown length. utf8-value store byte value for the number of utf8-length.

### Union

`((byte)254, byte tag, value)`

Union has 2 byte unsgined byte in header, First byte `254` is marker as Union, next unsgined byte is tag that for discriminated value type. When marker byte is `255` represents null.

License
---
This library is licensed under the MIT License.
