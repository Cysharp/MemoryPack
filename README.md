# MemoryPack
[![GitHub Actions](https://github.com/Cysharp/MemoryPack/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/MemoryPack/actions) [![Releases](https://img.shields.io/github/release/Cysharp/MemoryPack.svg)](https://github.com/Cysharp/MemoryPack/releases)

Zero encoding extreme performance binary serializer for C#.

// TODO: Benchmark image.

// TODO: Intro message.

Installation
---
This library is distributed via NuGet. Minimum requirement is `.NET 7 RC1` and Roslyn Incremental Generator(`4.4.0-1.final`) support.

> PM> Install-Package [MemoryPack](https://www.nuget.org/packages/MemoryPack)

And you need to enable preview features to `.csproj`.

```xml
<EnablePreviewFeatures>True</EnablePreviewFeatures>
```

Quick Start
---
Define the struct or class to be serialized and annotate it with a `[MemoryPackable]` attribute and `partial` keyword.

```csharp
[MemoryPackable]
public partial class MyClass
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
}
```

MemoryPack code generator generates `IMemoryPackable<T>` static abstract member.

In Visual Studio, you can check generated code via `Ctrl+K, R` on class name and select `*.MemoryPackFormatter.g.cs`.

Call `MemoryPackSerializer.Serialize<T>/Deserialize<T>` to serialize/deserialize your object instance.

```csharp
var v = new MyClass { MyProperty1 = 10, MyProperty2 = 40 };

var bin = MemoryPackSerializer.Serialize(v);
var v2 = MemoryPackSerializer.Deserialize<MyClass>(bin);
```

Built-in supported types
---


Code Generator Options
---

Serialization Callback
---

Union
---

Serialize API
---

Deserialize API
---


Performance
---


Payload size and compression
---






Streaming Serialization
---


Formatter API
---

Unity support
---


Binary wire format specification
---
The type of `T` defined in `Serialize<T>` and `Deserialize<T>` is called C# schema. MemoryPack format is not self described format. Deserialize requires the corresponding C# schema. Four types exist as internal representations of binaries, but types cannot be determined without a C# schema.

There are no endian specifications. It is not possible to convert on machines with different endianness. However modern computers are usually little-endian.

There are four types of format.

* Unmanaged struct
* Object
* Collection
* Union

### Unmanaged struct

### Object


### Collection

collectoin is 4byte unsigned interger as data count



### Union


License
---
This library is licensed under the MIT License.
