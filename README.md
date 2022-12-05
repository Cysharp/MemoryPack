# MemoryPack
[![GitHub Actions](https://github.com/Cysharp/MemoryPack/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/MemoryPack/actions) [![Releases](https://img.shields.io/github/release/Cysharp/MemoryPack.svg)](https://github.com/Cysharp/MemoryPack/releases)

Zero encoding extreme performance binary serializer for C# and Unity.

![image](https://user-images.githubusercontent.com/46207/200979655-63ed38ae-dad2-4ca0-bbb7-9e0aa98914af.png)

> Compare with [System.Text.Json](https://learn.microsoft.com/ja-jp/dotnet/api/system.text.json), [protobuf-net](https://github.com/protobuf-net/protobuf-net), [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp), [Orleans.Serialization](https://github.com/dotnet/orleans/). Measure by .NET 7 / Ryzen 9 5950X machine. These serializers has `IBufferWriter<byte>` method, serialize using `ArrayBuffer<byte>` and reuse to avoid measure buffer copy. 

For standard object, MemoryPack is x10 faster and x2 ~ x5 faster than other binary serializers. For struct array, MemoryPack gots boosted power, x50 ~ x200 faster than other serializers.

MemoryPack is my 4th serializer, previously I've created well known serializers, ~~[ZeroFormatter](https://github.com/neuecc/ZeroFormatter)~~, ~~[Utf8Json](https://github.com/neuecc/Utf8Json)~~, [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp). The reason for MemoryPack's speed is due to its C#-specific, C#-optimized binary format and a well tuned implementation based on my past experience. It is also a completely new design utilizing .NET 7 and C# 11 and the Incremental Source Generator(.NET Standard 2.1(.NET 5, 6) and Unity support is also exists).

Other serializers performs many encoding operations such as VarInt encoding, tag, string, etc. MemoryPack format uses a zero-encoding design that copies as much of the C# memory as possible. zero-encoding is similar as FlatBuffers but don't need special type, MemoryPack's serialize target is POCO.

Other than performance, MemoryPack has these features.

* Support modern I/O APIs(`IBufferWriter<byte>`, `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>`)
* Native AOT friendly Source Generator based code generation, no Dynamic CodeGen(IL.Emit)
* Reflectionless non-generics APIs
* Deserialize into existing instance
* Polymorphism(Union) serialization
* limited version-tolerant(fast/default) and full version-tolerant support
* Circular reference serialization
* PipeWriter/Reader based streaming serialization
* TypeScript code generation and ASP.NET Core Formatter
* Unity(2021.3) IL2CPP Support via .NET Source Generator

Installation
---
This library is distributed via NuGet. For best performance, recommend to use `.NET 7`. Minimum requirement is `.NET Standard 2.1`.

> PM> Install-Package [MemoryPack](https://www.nuget.org/packages/MemoryPack)

And also editor requires Roslyn 4.3.1 support, for example Visual Studio 2022 version 17.3, .NET SDK 6.0.401. For details, see [Roslyn Version Support](https://learn.microsoft.com/en-us/visualstudio/extensibility/roslyn-version-support) document.

For Unity, the requirements and installation process are completely different. See the [Unity](#unity) section for details.

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
* Unmanaged types(Any `enum`, Any user-defined `struct` that no contains reference type)
* `string`, `decimal`, `Half`, `Int128`, `UInt128`, `Guid`, `Rune`, `BigInteger`
* `TimeSpan`,  `DateTime`, `DateTimeOffset`, `TimeOnly`, `DateOnly`, `TimeZoneInfo`
* `Complex`, `Plane`, `Quaternion` `Matrix3x2`, `Matrix4x4`, `Vector2`, `Vector3`, `Vector4`
* `Uri`, `Version`, `StringBuilder`, `Type`, `BitArray`
* `T[]`, `T[,]`, `T[,,]`, `T[,,,]`, `Memory<>`, `ReadOnlyMemory<>`, `ArraySegment<>`, `ReadOnlySequence<>`
* `Nullable<>`, `Lazy<>`, `KeyValuePair<,>`, `Tuple<,...>`, `ValueTuple<,...>`
* `List<>`, `LinkedList<>`, `Queue<>`, `Stack<>`, `HashSet<>`, `SortedSet<>`, `PriorityQueue<,>`
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
* If chosen parameterized constructor, all parameter name must match with member name(case-insensitive)

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

When serialize, deserialize, MemoryPack can hook before/after event with `[MemoryPackOnSerializing]`, `[MemoryPackOnSerialized]`, `[MemoryPackOnDeserializing]`, `[MemoryPackOnDeserialized]` attributes. It can annotate both static and instance, public and private method but must be parameterless method.

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

`tag` allows `0` ~ `65535`, it is especially efficient for less than `250`.

If interface and derived type is in different assembly, you can use `MemoryPackUnionFormatterAttribute` instead.

```csharp
// AssemblyA
[MemoryPackable(GenerateType.NoGenerate)]
public partial interface IUnionSample
{
}

// AssemblyB define definition outsiede of target type
[MemoryPackUnionFormatter(typeof(IUnionSample))]
[MemoryPackUnion(0, typeof(FooClass))]
[MemoryPackUnion(1, typeof(BarClass))]
public partial class UnionSampleFormatter
{
}
```

> Formatter is register automatically via `ModuleInitializer` in C# 9.0. However ModuleInitializer is not supported in Unity, so if you use in Unity, invoke `***Initializer.RegisterFormatter()` manually in Startup. For example `UnionSampleFormatterInitializer.RegisterFormatter()`.

Union can assembly in code via `DynamicUnionFormatter<T>`.

```csharp
var formatter = new DynamicUnionFormatter<IFooBarBaz>(new[]
{
    (0, typeof(Foo)),
    (1, typeof(Bar)),
    (2, typeof(Baz))
});

MemoryPackFormatterProvider.Register(formatter);
```

Serialize API
---
Serialize has three overloads.

```csharp
// Non generic API also available, these version is first argument is Type and value is object?
byte[] Serialize<T>(in T? value, MemoryPackSerializerOptions? options = default)
void Serialize<T, TBufferWriter>(in TBufferWriter bufferWriter, in T? value, MemoryPackSerializerOptions? options = default)
async ValueTask SerializeAsync<T>(Stream stream, T? value, MemoryPackSerializerOptions? options = default, CancellationToken cancellationToken = default)
```

The recommended way to do this in Performance is to use `BufferWriter`. This serializes directly into the buffer. It can be applied to `PipeWriter` in `System.IO.Pipelines`, `BodyWriter` in ASP .NET Core, etc.

If a `byte[]` is required (e.g. `RedisValue` in [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)), return `byte[]` API is simple and mostly fast.

Note that `SerializeAsync` for `Stream` is asynchronous only for Flush; it serializes everything once into MemoryPack's internal pool buffer and then writes it out with WriteAsync. Therefore, BufferWriter overloading, which separates and controls buffer and flush, is better.

If you want to do complete streaming write, see [Streaming Serialization](#streaming-serialization) section.

### MemoryPackSerializerOptions

`MemoryPackSerializerOptions` configures how serialize string as Utf16 or Utf8. If passing null then uses `MemoryPackSerializerOptions.Default`, it is same as `MemoryPackSerializerOptions.Utf8`, in other words, serialize the string as Utf8. If you want to serialize with Utf16, you can use `MemoryPackSerializerOptions.Utf16`.

Since C#'s internal string representation is UTF16, UTF16 performs better. However, the payload tends to be larger; in UTF8, an ASCII string is one byte, while in UTF16 it is two bytes. Because the difference in size of this payload is so large, UTF8 is set by default.

If the data is non-ASCII (e.g. Japanese, which can be more than 3 bytes, and UTF8 is larger), or if you have to compress it separately, UTF16 may give better results.

Whether UTF8 or UTF16 is selected during serialization, it is not necessary to specify it during deserialization. It will be automatically detected and deserialized normally.

Deserialize API
---
Deserialize has `ReadOnlySpan<byte>` and `ReadOnlySequence<byte>`, `Stream` overload and `ref` support.

```csharp
T? Deserialize<T>(ReadOnlySpan<byte> buffer)
int Deserialize<T>(ReadOnlySpan<byte> buffer, ref T? value)
T? Deserialize<T>(in ReadOnlySequence<byte> buffer)
int Deserialize<T>(in ReadOnlySequence<byte> buffer, ref T? value)
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
* only allows parameterless constructor, if parameterized constructor is used, create new instance
* if value is `T[]`, reuse only if the length is the same, otherwise create new instance
* if value is collection that has `.Clear()` method(`List<>`, `Stack<>`, `Queue<>`, `LinkedList<>`, `HashSet<>`, `PriorityQueue<,>`, `ObservableCollection`, `Collection`, `ConcurrentQueue<>`, `ConcurrentStack<>`, `ConcurrentBag<>`, `Dictionary<,>`, `SoretedDictionary<,>`, `SortedList<,>`, `ConcurrentDictionary<,>`) call Clear() and reuse it, otherwise create new instance

Version tolerant
---
In default(`GeneratType.Object`), MemoryPack supports schema evolution limitedly.

* unmanaged struct can't change any more
* members can add, but can not delete
* can change member name
* can't change member order
* can't change member type

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

In use-case, store old data(to file, to redis, etc...) and read to new schema is always ok. In RPC scenario, schema exists both client server, the client must be updated before the server. An updated client has no problem connecting to the old server but old client can not connect to new server.

Next [Serialization info](#serialization-info) section shows how to check for schema changes, e.g., by CI, to prevent accidents.

When using `GnerateType.VersionTolerant`, it supports full version-tolerant.

* unmanaged struct can't change any more
* All members must add `[MemoryPackOrder]` explicitly
* members can add, can delete but not reuse order(can use missing order)
* can change member name
* can't change member order
* can't change member type

```csharp
// Ok to serialize/deserialize both 
// VersionTolerantObject1 -> VersionTolerantObject2 and 
// VersionTolerantObject2 -> VersionTolerantObject1

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class VersionTolerantObject1
{
    [MemoryPackOrder(0)]
    public int MyProperty0 { get; set; } = default;

    [MemoryPackOrder(1)]
    public long MyProperty1 { get; set; } = default;

    [MemoryPackOrder(2)]
    public short MyProperty2 { get; set; } = default;
}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class VersionTolerantObject2
{
    [MemoryPackOrder(0)]
    public int MyProperty0 { get; set; } = default;

    // deleted
    //[MemoryPackOrder(1)]
    //public long MyProperty1 { get; set; } = default;

    [MemoryPackOrder(2)]
    public short MyProperty2 { get; set; } = default;

    // added
    [MemoryPackOrder(3)]
    public short MyProperty3 { get; set; } = default;
}
```

`GnerateType.VersionTolerant` is slower than `GeneratType.Object` in serializing. Also, the payload size will be slightly larger.

Serialization info
----
Which members are serialized, you can check IntelliSense in type. There is an option to write that information to a file at compile time. Set `MemoryPackGenerator_SerializationInfoOutputDirectory` as follows.

```xml
<!-- output memorypack serialization info to directory -->
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

Circular Reference
---
MemoryPack also supports circular reference. This allows the tree objects to be serialized as is.

```csharp
// to enable circular-reference, use GenerateType.CircularReference
[MemoryPackable(GenerateType.CircularReference)]
public partial class Node
{
    [MemoryPackOrder(0)]
    public Node? Parent { get; set; }
    [MemoryPackOrder(1)]
    public Node[]? Children { get; set; }
}
```

 For example, [System.Text.Json preserve-references](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references) code will become like here.

```csharp
// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references?pivots=dotnet-7-0
Employee tyler = new()
{
    Name = "Tyler Stein"
};

Employee adrian = new()
{
    Name = "Adrian King"
};

tyler.DirectReports = new List<Employee> { adrian };
adrian.Manager = tyler;

var bin = MemoryPackSerializer.Serialize(tyler);
Employee? tylerDeserialized = MemoryPackSerializer.Deserialize<Employee>(bin);

Console.WriteLine(tylerDeserialized?.DirectReports?[0].Manager == tylerDeserialized); // true

[MemoryPackable(GenerateType.CircularReference)]
public partial class Employee
{
    [MemoryPackOrder(0)]
    public string? Name { get; set; }
    [MemoryPackOrder(1)]
    public Employee? Manager { get; set; }
    [MemoryPackOrder(2)]
    public List<Employee>? DirectReports { get; set; }
}
```

`GenerateType.CircularReference` has the same characteristics as version-tolerant. However, as an additional constraint, only parameterless constructors are allowed. Also, object reference tracking is only done for objects marked with `GenerateType.CircularReference`. If you want to track any other object, wrap it.

CustomFormatter
---
If implements `MemoryPackCustomFormatterAttribute<T>` or `MemoryPackCustomFormatterAttribute<TFormatter, T>`(more performant but complex), you can configure to use custom formatter to MemoryPackObject's member.

```csharp
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public abstract class MemoryPackCustomFormatterAttribute<T> : Attribute
{
    public abstract IMemoryPackFormatter<T> GetFormatter();
}
```

In built-in attribtues, `Utf8StringFormatterAttribute`, `Utf16StringFormatterAttribute`, `InternStringFormatterAttribute`, `OrdinalIgnoreCaseStringDictionaryFormatterAttribtue<TValue>`, `BitPackFormatterAttribtue`, `BrotliFormatter`, `BrotliStringFormatter`, `BrotliFormatter<T>`, `MemoryPoolFormatter<T>`, `ReadOnlyMemoryPoolFormatter<T>` exists.

```csharp
[MemoryPackable]
public partial class Sample
{
    // serialize this member as UTF16 String, it is performant than UTF8 but in ASCII, size is larger(but non ASCII, sometimes smaller).
    [Utf16StringFormatter]
    public string? Text { get; set; }

    // In deserialize, Dictionary is initialized with StringComparer.OrdinalIgnoreCase.
    [OrdinalIgnoreCaseStringDictionaryFormatter<int>]
    public Dictionary<string, int>? Ids { get; set; }
    
    // In deserialize time, all string is interned(see: String.Intern). If similar values come repeatedly, it saves memory.
    [InternStringFormatter]
    public string? Flag { get; set; }
}
```

For configure set/dictionary's equality comparer, all built-in formatter has comparer constructor overload. You can create custom equality-comparer formatter easily.

```csharp
public sealed class OrdinalIgnoreCaseStringDictionaryFormatter<TValue> : MemoryPackCustomFormatterAttribute<Dictionary<string, TValue?>>
{
    static readonly DictionaryFormatter<string, TValue?> formatter = new DictionaryFormatter<string, TValue?>(StringComparer.OrdinalIgnoreCase);

    public override IMemoryPackFormatter<Dictionary<string, TValue?>> GetFormatter()
    {
        return formatter;
    }
}
```

`BitPackFormatter` is for `bool[]`, same serialized result as `BitArray`. In other words, bool is normally 1byte, but since it is treated as 1bit, eight bools are stored in one byte. Therefore, the size after serialization is 1/8.

```csharp
[MemoryPackable]
public partial class Sample
{
    public int Id { get; set; }

    [BitPackFormatter]
    public bool[]? Data { get; set; }
}
```

`BrotliFormatter` is for `byte[]`, for example you can compress large payload by Brotli.

```csharp
[MemoryPackable]
public partial class Sample
{
    public int Id { get; set; }

    [BrotliFormatter]
    public byte[]? Payload { get; set; }
}
```

`BrotliStringFormatter` is for `string`, serialize compressed string(UTF16) by Brotli.

```csharp
[MemoryPackable]
public partial class Sample
{
    public int Id { get; set; }

    [BrotliStringFormatter]
    public string? LargeText { get; set; }
}
```

`BrotliFormatter<T>` is for any type, serialized data compressed by Brotli. If type is `byte[]` or `string`, should use `BrotliFormatter` or `BrotliStringFormatter` for performance.

```csharp
[MemoryPackable]
public partial class Sample
{
    public int Id { get; set; }

    [BrotliFormatter<ChildType>]
    public ChildType? Child { get; set; }
}
```

Deserialize array pooling
---
For deserializing large array(any `T`), MemoryPack offers multiple efficient pooling methods. The most effective way is to use the [#Overwrite](#overwrite) function. In particular, with `List<T>`, always reuse.

```csharp
[MemoryPackable]
public partial class ListBytesSample
{
    public int Id { get; set; }
    public List<byte> Payload { get; set; }
}

// ----

// List<byte> is reused, no allocation in deserialize.
MemoryPackSerializer.Deserialize<ListBytesSample>(bin, ref reuseObject);

// for efficient operation, you can get Span<T> by CollectionsMarshal
var span = CollectionsMarshal.AsSpan(value.Payload);
```

A convenient way is to deserialize to an ArrayPool at deserialization time. MemoryPack provides `MemoryPoolFormatter<T>` and `ReadOnlyMemoryPoolFormatter<T>`.

```csharp
[MemoryPackable]
public partial class PoolModelSample : IDisposable
{
    public int Id { get; }

    [MemoryPoolFormatter<byte>]
    public Memory<byte> Payload { get; private set; }

    public PoolModelSample(int id, Memory<byte> payload)
    {
        Id = id;
        Payload = payload;
    }

    // You must write the return code yourself, here is snippet.

    bool usePool;

    [MemoryPackOnDeserialized]
    void OnDeserialized()
    {
        usePool = true;
    }

    public void Dispose()
    {
        if (!usePool) return;

        Return(Payload); Payload = default;
    }

    static void Return<T>(Memory<T> memory) => Return((ReadOnlyMemory<T>)memory);

    static void Return<T>(ReadOnlyMemory<T> memory)
    {
        if (MemoryMarshal.TryGetArray(memory, out var segment) && segment.Array is { Length: > 0 })
        {
            ArrayPool<T>.Shared.Return(segment.Array, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }
}

// ---

using(var value = MemoryPackSerializer.Deserialize<PoolModelSample>(bin))
{
    // do anything...
}   // return to ArrayPool
```

Performance
---
See the my blog post [How to make the fastest .NET Serializer with .NET 7 / C# 11, case of MemoryPack](https://medium.com/@neuecc/how-to-make-the-fastest-net-serializer-with-net-7-c-11-case-of-memorypack-ad28c0366516)

Payload size and compression
---
Payload size depends on the target value; unlike JSON, there are no keys and it is a binary format, so the payload size is likely to be smaller than JSON.

For those with variant encoding, such as MessagePack and Protobuf, MemoryPack tends to be larger if ints are used a lot (in MemoryPack, ints are always 4 bytes due to fixed size encoding, while MsgPack is 1~5 bytes).

float and double are 4 bytes and 8 bytes in MemoryPack, but 5 bytes and 9 bytes in MsgPack. So MemoryPack is smaller, for example, for Vector3 (float, float, float) arrays.

String is UTF8 by default, which is similar to other serializers, but if the UTF16 option is chosen, it will be of a different nature.

In any case, if the payload size is large, compression should be considered. LZ4, ZStandard and Brotli are recommended.

### Compression

MemoryPack provides an efficient helper for [Brotli](https://github.com/google/brotli) compression via [BrotliEncoder](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.brotliencoder) and [BrotliDecoder](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.brotlidecoder). MemoryPack's `BrotliCompressor` and `BrotliDecompressor` provide compression/decompression optimized for MemoryPack's internal behavior.

```csharp
using MemoryPack.Compression;

// Compression(require using)
using var compressor = new BrotliCompressor();
MemoryPackSerializer.Serialize(compressor, value);

// Get compressed byte[]
var compressedBytes = compressor.ToArray();

// Or write to other IBufferWriter<byte>(for example PipeWriter)
compressor.CopyTo(response.BodyWriter);
```

```csharp
using MemoryPack.Compression;

// Decompression(require using)
using var decompressor = new BrotliDecompressor();

// Get decompressed ReadOnlySequence<byte> from ReadOnlySpan<byte> or ReadOnlySequence<byte>
var decompressedBuffer = decompressor.Decompress(buffer);

var value = MemoryPackSerializer.Deserialize<T>(decompressedBuffer);
```

Both `BrotliCompressor` and `BrotliDecompressor` are struct, it does not allocate memory on heap. Both store compressed or decompressed data in an internal memory pool for Serialize/Deserialize.  Therefore, it is necessary to release the memory pooling, don't forget to use `using`.

Compression level is very important. The default is set to quality-1 (CompressionLevel.Fastest), which is different from the .NET default (CompressionLevel.Optimal, quality-4).

Fastest (quality-1) will be close to the speed of [LZ4](https://github.com/lz4/lz4), but 4 is much slower. This was determined to be critical in the serializer use scenario. Be careful when using the standard `BrotliStream`(quality-4 is the default). In any case, compression/decompression speeds and sizes will result in very different results for different data. Please prepare the data to be handled by your application and test it yourself.

Note that there is a several-fold speed penalty between MemoryPack's uncompressed and Brotli's added compression.

Brotli support also exists in custom formatter. `BrotliFormatter` can compress specify member.

```csharp
[MemoryPackable]
public partial class Sample
{
    public int Id { get; set; }

    [BrotliFormatter]
    public byte[]? Payload { get; set; }
}
```

Serialize external types
---
If you want to serialize external types, you can make custom formatter and register to provider, see [Formatter/Provider API](#formatterprovider-api) for details. However, creating a custom formatter is difficult. Therefore, we recommend making a wrapper type. For example, if you want to serialize an external type called `AnimationCurve`.

```csharp
// Keyframe: (float time, float inTangent, float outTangent, int tangentMode, int wightedMode, float inWeight, float outWeight)
[MemoryPackable]
public readonly partial struct SerializableAnimationCurve
{
    [MemoryPackIgnore]
    public readonly AnimationCurve AnimationCurve;

    [MemoryPackInclude]
    WrapMode preWrapMode => AnimationCurve.preWrapMode;
    [MemoryPackInclude]
    WrapMode postWrapMode => AnimationCurve.postWrapMode;
    [MemoryPackInclude]
    Keyframe[] keys => AnimationCurve.keys;

    [MemoryPackConstructor]
    SerializableAnimationCurve(WrapMode preWrapMode, WrapMode postWrapMode, Keyframe[] keys)
    {
        var curve = new AnimationCurve(keys);
        curve.preWrapMode = preWrapMode;
        curve.postWrapMode = postWrapMode;
        this.AnimationCurve = curve;
    }

    public SerializableAnimationCurve(AnimationCurve animationCurve)
    {
        this.AnimationCurve = animationCurve;
    }
}
```

The type to wrap is public, but excluded from serialization(`MemoryPackIgnore`). The properties you want to serialize are private, but included (`MemoryPackInclude`). Two patterns of constructors should also be prepared. The constructor used by the serializer should be private.

As it is, I have to wrap it every time, which is inconvenient. Let's create a custom formatter.

```csharp
public class AnimationCurveFormatter : MemoryPackFormatter<AnimationCurve>
{
    // If Unity that does not support scoped and TBufferWriter so change signature to `Serialize(ref MemoryPackWriter writer, ref AnimationCurve value)`
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref AnimationCurve value)
    {
        writer.WritePackable(new SerializableAnimationCurve(value));
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref AnimationCurve value)
    {
        var wrapped = reader.ReadPackable<SerializableAnimationCurve>();
        value = wrapped.AnimationCurve;
    }
}
```

Finally, register formatter in startup.

```csharp
MemoryPackFormatterProvider.Register<AnimationCurve>(new AnimationCurveFormatter());
```

Packages
---
MemoryPack has thesed packages.

* MemoryPack
* MemoryPack.Core
* MemoryPack.Generator
* MemoryPack.Streaming
* MemoryPack.AspNetCoreMvcFormatter

Mainly you only reference `MemoryPack`, this both `MemoryPack.Core` and `MemoryPack.Generator`. If you want to use [Streaming Serialization](#streaming-serialization), additionaly use `MemoryPack.Streaming`. `MemoryPack.AspNetCoreMvcFormatter` is input/output formatter for ASP.NET Core.

TypeScript and ASP.NET Core Formatter
---
MemoryPack supports TypeScript code generation. It generates class and serialization code from C#, In other words, you can share types with the Browser without using OpenAPI, proto, etc.

Code generation is integrated with Source Generator, the following options(`MemoryPackGenerator_TypeScriptOutputDirectory`) set the output directory for TypeScript code. Runtime code is output at the same time, so no additional dependencies are required.

```xml
<!-- output memorypack TypeScript code to directory -->
<ItemGroup>
    <CompilerVisibleProperty Include="MemoryPackGenerator_TypeScriptOutputDirectory" />
</ItemGroup>
<PropertyGroup>
    <MemoryPackGenerator_TypeScriptOutputDirectory>$(MSBuildProjectDirectory)\wwwroot\js\memorypack</MemoryPackGenerator_TypeScriptOutputDirectory>
</PropertyGroup>
```

At first, require to annotate `[GenerateTypeScript]` to C# MemoryPackable type.

```csharp
[MemoryPackable]
[GenerateTypeScript]
public partial class Person
{
    public required Guid Id { get; init; }
    public required int Age { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateTime DateOfBirth { get; init; }
    public required Gender Gender { get; init; }
    public required string[] Emails { get; init; }
}

public enum Gender
{
    Male, Female, Other
}
```

Runtime code and TypeScript type will generate to target directory.

![image](https://user-images.githubusercontent.com/46207/194916544-1b6bb5ed-966b-43c3-a378-3eac297c2b40.png)

The generated code is as follows, with simple fields and static methods for serialize/serializeArray and deserialize/deserializeArray.

```typescript
import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { Gender } from "./Gender.js"; 

export class Person {
    id: string;
    age: number;
    firstName: string | null;
    lastName: string | null;
    dateOfBirth: Date;
    gender: Gender;
    emails: (string | null)[] | null;

    constructor() {
        // snip...
    }

    static serialize(value: Person | null): Uint8Array {
        // snip...
    }

    static serializeCore(writer: MemoryPackWriter, value: Person | null): void {
        // snip...
    }

    static serializeArray(value: (Person | null)[] | null): Uint8Array {
        // snip...
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (Person | null)[] | null): void {
        // snip...
    }
    static deserialize(buffer: ArrayBuffer): Person | null {
        // snip...
    }

    static deserializeCore(reader: MemoryPackReader): Person | null {
        // snip...
    }

    static deserializeArray(buffer: ArrayBuffer): (Person | null)[] | null {
        // snip...
    }

    static deserializeArrayCore(reader: MemoryPackReader): (Person | null)[] | null {
        // snip...
    }
}
```

You can use this type like following.

```typescript
let person = new Person();
person.id = crypto.randomUUID();
person.age = 30;
person.firstName = "foo";
person.lastName = "bar";
person.dateOfBirth = new Date(1999, 12, 31, 0, 0, 0);
person.gender = Gender.Other;
person.emails = ["foo@bar.com", "zoo@bar.net"];

// serialize to Uint8Array
let bin = Person.serialize(person);

let blob = new Blob([bin.buffer], { type: "application/x-memorypack" })

let response = await fetch("http://localhost:5260/api",
    { method: "POST", body: blob, headers: { "Content-Type": "application/x-memorypack" } });

let buffer = await response.arrayBuffer();

// deserialize from ArrayBuffer 
let person2 = Person.deserialize(buffer);
```

We're also providing `MemoryPack.AspNetCoreMvcFormatter` package, you can add `MemoryPackInputFormatter`, `MemoryPackOutputFormatter` to ASP.NET Core MVC.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddControllers(options =>
{
    options.InputFormatters.Insert(0, new MemoryPackInputFormatter());
    // If checkContentType: true then can output multiple format(JSON/MemoryPack, etc...). default is false.
    options.OutputFormatters.Insert(0, new MemoryPackOutputFormatter(checkContentType: false));
});
```

If you call from HttpClient, you can set `application/x-memorypack` to content-header.

```csharp
var content = new ByteArrayContent(bin)
content.Headers.ContentType = new MediaTypeHeaderValue("application/x-memorypack");
```

### TypeScript Type Mapping

There are a few restrictions on the types that can be generated. Among the primitives, `char` and `decimal` are not supported. Also, OpenGenerics type cannot be used.

|  C#  |  TypeScript  | Description |
| ---- | ---- | ---- |
| `bool` |  `boolean`  |
| `byte` |  `number`  |
| `sbyte` |  `number`  |
| `int` |  `number` |
| `uint` |  `number` |
| `short` |  `number` |
| `ushort` |  `number` |
| `long` |  `bigint` |
| `ulong` |  `bigint` |
| `float` |  `number` |
| `double` |  `number` |
| `string` |  `string \| null`  | 
| `Guid` |  `string`  | In TypeScript, represents as string but serialize/deserialize as 16byte binary
| `DateTime` | `Date` | DateTimeKind will be ignored
| `enum` | `const enum` | `long` and `ulong` underlying type is not supported
| `T?` | `T \| null` |
| `T[]` | `T[] \| null` |
| `byte[]` | `Uint8Array \| null` |
| `: ICollection<T>` | `T[] \| null` | Supports all `ICollection<T>` implemented type like `List<T>`
| `: ISet<T>` | `Set<T> \| null` | Supports all `ISet<T>` implemented type like `HashSet<T>`
| `: IDictionary<K,V>` | `Map<K, V> \| null` | Supports all `IDictionary<K,V>` implemented type like `Dictionary<K,V>`.
| `[MemoryPackable]` | `class` | Supports class only
| `[MemoryPackUnion]` | `abstract class` |

`[GenerateTypeScript]` can only be applied to classes and is currently not supported by struct.

### Configure import file extension and member name casing

In default, MemoryPack generates file extension as `.js` like `import { MemoryPackWriter } from "./MemoryPackWriter.js";`. If you want to change other extension or empty, use `MemoryPackGenerator_TypeScriptImportExtension` to configure it.
Also member name is automatically convert to lowerCase. If you want to use original name, use `MemoryPackGenerator_TypeScriptConvertPropertyName` to `false`.

```xml
<ItemGroup>
    <CompilerVisibleProperty Include="MemoryPackGenerator_TypeScriptOutputDirectory" />
    <CompilerVisibleProperty Include="MemoryPackGenerator_TypeScriptImportExtension" />
    <CompilerVisibleProperty Include="MemoryPackGenerator_TypeScriptConvertPropertyName" />
</ItemGroup>
<PropertyGroup>
    <MemoryPackGenerator_TypeScriptOutputDirectory>$(MSBuildProjectDirectory)\wwwroot\js\memorypack</MemoryPackGenerator_TypeScriptOutputDirectory>
    <!-- allows empty -->
    <MemoryPackGenerator_TypeScriptImportExtension></MemoryPackGenerator_TypeScriptImportExtension>
    <!-- default is true -->
    <MemoryPackGenerator_TypeScriptConvertPropertyName>false</MemoryPackGenerator_TypeScriptConvertPropertyName>
</PropertyGroup>
```

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

Target framework dependency
---
MemoryPack provides `netstandard2.1` and `net7.0` but both are not compatibility. For example, MemoryPackable types under `netstandard2.1` project and use it from `net7.0` project, throws runtime exception like this

> Unhandled exception. System.TypeLoadException: Virtual static method '*' is not implemented on type '*' from assembly '*'.

Since net7.0 uses static abstract members(`Virtual static method`), that does not support netstandard2.1, this behavior is a specification.

.NET 7 project can't refer netstandard 2.1 dll. In other words, if the Application is a .NET 7 Project, all of the dependencies that use MemoryPack must support .NET 7. So for library developer if depend MemoryPack, you need to configure dual target framework.

```xml
<TargetFrameworks>netstandard2.1;net7.0</TargetFrameworks>
```

Unity
---
Install via UPM git URL package or asset package(MemoryPack.*.*.*.unitypackage) available in [MemoryPack/releases](https://github.com/Cysharp/MemoryPack/releases) page.

* https://github.com/Cysharp/MemoryPack.git?path=src/MemoryPack.Unity/Assets/Plugins/MemoryPack

If you want to set a target version, MemoryPack uses the `*.*.*` release tag so you can specify a version like #1.8.0. For example `https://github.com/Cysharp/MemoryPack.git?path=src/MemoryPack.Unity/Assets/Plugins/MemoryPack#1.8.0`.

Supporting minimum Unity version is `2021.3`. The dependency managed DLL `System.Runtime.CompilerServices.Unsafe/6.0.0` is included with unitypackage. For git references, you will need to add them in another way as they are not included to avoid unnecessary dependencies; either extract the dll from unitypackage or download it from the [NuGet page](https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.0.0).

As with the .NET version, the code is generated by a code generator(`MemoryPack.Generator.Roslyn3.dll`). Reflection-free implementation also provides the best performance in IL2CPP.

For more information on Unity and Source Generator, please refer to the [Unity documentation](https://docs.unity3d.com/Manual/roslyn-analyzers.html).

Source Generator is also used officially by Unity by [com.unity.properties](https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/index.html) and [com.unity.entities](https://docs.unity3d.com/Packages/com.unity.properties@2.0/changelog/CHANGELOG.html). In other words, it is the standard for code generation in the next generation of Unity.

Unity version is not supported CustomFormatter and ImmutableCollections.

You can serializer all unmanaged types(such as `Vector3`, `Rect`, etc...). If you want to serialize other Unity-specific types (e.g., `AnimationCurve`), see [Serialize external types](#serialize-external-types) section.

Native AOT
---
Unfortunately, .NET 7 Native AOT causes crash(`Generic virtual method pointer lookup failure`) when use MemoryPack due to a runtime bug. It will fix on .NET 8 and see the reporing issue for workaround(write XML manually). [Generic virtual method pointer lookup failure in .NET 7 Native AOT #78882](https://github.com/dotnet/runtime/issues/78882).

Binary wire format specification
---
The type of `T` defined in `Serialize<T>` and `Deserialize<T>` is called C# schema. MemoryPack format is not self described format. Deserialize requires the corresponding C# schema. These types exist as internal representations of binaries, but types cannot be determined without a C# schema.

Endian must be `Little Endian`. However reference C# implementation does not care endianness so can not use on big-endian machine. However modern computers are usually little-endian.

There are eight types of format.

* Unmanaged struct
* Object
* Version Tolerant Object
* Circular Reference Object
* Tuple
* Collection
* String
* Union

### Unmanaged struct

Unmanaged struct is C# struct that no contains reference type, similar constraint of [C# Unmanaged types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types). Serializing struct layout as is, includes padding.

### Object

`(byte memberCount, [values...])`

Object has 1byte unsigned byte as member count in header. Member count allows `0` to `249`, `255` represents object is `null`. Values store memorypack value for the number of member count.

### Version Tolerant Object

`(byte memberCount, [varint byte-length-of-values...], [values...])`

Version Tolerant Object is similar as Object but has byte length of values in header. varint follows these spec, first sbyte is value or typeCode and next X byte is value. 0 to 127 = unsigned byte value, -1 to -120 = signed byte value, -121 = byte, -122 = sbyte, -123 = ushort, -124 = short, -125 = uint, -126 = int, -127 = ulong, -128 = long.

### Circular Reference Object

`(byte memberCount, [varint byte-length-of-values...], varint referenceId, [values...])`  
`(250, varint referenceId)`

Circular Reference Object is similar as Version Tolerant Object but if memberCount is 250, next varint(unsigned-int32) is referenceId. If not, after byte-length-of-values, varint referenceId is written.

### Tuple

`(values...)`

Tuple is fixed-size, non-nullable value collection. In .NET, `KeyValuePair<TKey, TValue>` and `ValueTuple<T,...>` are serialized as Tuple.

### Collection

`(int length, [values...])`

Collection has 4byte signed integer as data count in header, `-1` represents `null`. Values store memorypack value for the number of length.

### String

`(int utf16-length, utf16-value)`  
`(int ~utf8-byte-count, int utf16-length, utf8-bytes)`

String has two-forms, UTF16 and UTF8. If first 4byte signed integer is `-1`, represents null. `0`, represents empty. UTF16 is same as collection(serialize as `ReadOnlySpan<char>`, utf16-value's byte count is utf16-length * 2). If first signed integer <= `-2`, value is encoded by UTF8. utf8-byte-count is encoded in complement, `~utf8-byte-count` to retrieve count of bytes. Next signed integer is utf16-length, it allows `-1` that represents unknown length. utf8-bytes store bytes for the number of utf8-byte-count.

### Union

`(byte tag, value)`  
`(250, ushort tag, value)`

First unsigned byte is tag that for discriminated value type or flag, `0` to `249` represents tag, `250` represents next unsigned short is tag, `255` represents union is `null`.

License
---
This library is licensed under the MIT License.
