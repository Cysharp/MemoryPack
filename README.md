# MemoryPack
[![GitHub Actions](https://github.com/Cysharp/MemoryPack/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/MemoryPack/actions) [![Releases](https://img.shields.io/github/release/Cysharp/MemoryPack.svg)](https://github.com/Cysharp/MemoryPack/releases)

Zero encoding extreme performance binary serializer for C#.

// TODO: Benchmark image.

// TODO: Intro message.

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
[MemoryPackable]
public partial class Person
{
    public int Age { get; set; }
    public string Name { get; set; }
}
```

Serialization code is generated vis C# source generator feature, that implements `IMemoryPackable<T>` interface. In Visual Studio, you can check generated code via `Ctrl+K, R` on class name and select `*.MemoryPackFormatter.g.cs`.

Call `MemoryPackSerializer.Serialize<T>/Deserialize<T>` to serialize/deserialize your object instance.

```csharp
var v = new Person { Age = 40, Name = "John" };

var bin = MemoryPackSerializer.Serialize(v);
var val = MemoryPackSerializer.Deserialize<Person>(bin);
```

Serialize method supports return `byte[]` and serialize to `IBufferWriter<byte>` or `Stream`. Deserialize method supports `ReadOnlySpan<byte>`, `ReadOnlySeqeunce<byte>` and `Stream`. Andalso there have non-generics version.

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
* `List<>`, `LinkedList<>`, `Queue<>`, `Stack<>`, `HashSet<>`, `PriorityQueue<>`
* `Dictionary<,>`, `SortedList<,>`, `SortedDictionary<,>`,  `ReadOnlyDictionary<,>` 
* `Collection<>`, `ReadOnlyCollection<>`,`ObservableCollection<>`, `ReadOnlyObservableCollection<>`
* `IEnumerable<>`, `ICollection<>`, `IList<>`, `IReadOnlyCollection<>`, `IReadOnlyList<>`, `ISet<>`
* `IDictionary<,>`, `IReadOnlyDictionary<,>`, `ILookup<,>`, `IGrouping<,>`,
* `ConcurrentBag<>`, `ConcurrentQueue<>`, `ConcurrentStack<>`, `ConcurrentDictionary<,>`, `BlockingCollection<>`
* Immutable collections (`ImmutableList<>`, etc) and interfaces (`IImmutableList<>`, etc)

Define `[MemoryPackable]` `class` / `struct` / `record` / `record struct`
---








Options



Serialization Callback


Union
---

Serialize API
---

Deserialize API
---


Overwrite
---

// Clear support collection formatters
// T[], List, Stack, Queue, LinkedList, HashSet, PriorityQueue,
// ObservableCollection, Collection
// ConcurrentQueue, ConcurrentStack, ConcurrentBag
// Dictionary, SortedDictionary, SortedList, ConcurrentDictionary



Performance
---


Payload size and compression
---




Version tolerant
---

Packages
---

Serialization info
----
```
<!-- output memoerypack serialization info to directory -->
<ItemGroup>
    <CompilerVisibleProperty Include="MemoryPackGenerator_SerializationInfoOutputDirectory" />
</ItemGroup>
<PropertyGroup>
    <MemoryPackGenerator_SerializationInfoOutputDirectory>$(MSBuildProjectDirectory)MemoryPackLogs</MemoryPackGenerator_SerializationInfoOutputDirectory>
</PropertyGroup>
```


Streaming Serialization
---


Formatter API
---

Unity support
---
Currently MemoryPack dependents .NET 7 runtime, incremental generator and C# 11. Therefore it will not work in Unity. .NET 7 support is planned for Unity 2025.

If you request it, there is a possibility to make a detuned Unity version. Please send your request to Issues with the case you wish to use.

Binary wire format specification
---
The type of `T` defined in `Serialize<T>` and `Deserialize<T>` is called C# schema. MemoryPack format is not self described format. Deserialize requires the corresponding C# schema. Four types exist as internal representations of binaries, but types cannot be determined without a C# schema.

There are no endian specifications. It is not possible to convert on machines with different endianness. However modern computers are usually little-endian.

There are four value types of format.

* Unmanaged struct
* Object
* Collection
* Union

### Unmanaged struct

Unmanaged struct is C# struct that no contains referene type, similar constraint of [C# Unmanaged types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types). Serializing struct layout as is, includes padding.

### Object

`{byte memberCount, values...}`

Object has 1byte unsigned byte as member count in header. Member count allows `0` to `249`, `255` represents object is `null`. Values store memorypack value for the number of member count.


### Collection

`[int length, values...]`

Collection has 4byte signed interger as data count in header, `-1` represents `null`. Values store memorypack value for the number of length.

### Union

`((byte)254, byte tag, value)`

Union has 2 byte unsgined byte in header, First byte `254` is marker as Union, next unsgined byte is tag that for discriminated value type. 

License
---
This library is licensed under the MIT License.
