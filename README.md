# MemoryPack
[![GitHub Actions](https://github.com/Cysharp/MemoryPack/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/MemoryPack/actions) [![Releases](https://img.shields.io/github/release/Cysharp/MemoryPack.svg)](https://github.com/Cysharp/MemoryPack/releases)

Zero encoding extreme performance binary serializer for C#.

Currently preview.

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

wire format
---
* unmanaged struct
* object
* collection
* union

License
---
This library is licensed under the MIT License.
