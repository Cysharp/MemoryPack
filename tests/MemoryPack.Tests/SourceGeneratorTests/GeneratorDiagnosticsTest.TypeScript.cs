#if NET7_0_OR_GREATER

using MemoryPack.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public partial class GeneratorDiagnosticsTest
{
    void Compile2(int id, string code, bool allowMultipleError = false)
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator(code, options: new TypeScriptOptionProvider());
        if (!allowMultipleError)
        {
            diagnostics.Length.Should().Be(1);
            diagnostics[0].Id.Should().Be("MEMPACK" + id.ToString("000"));
        }
        else
        {
            diagnostics.Select(x => x.Id).Should().Contain("MEMPACK" + id.ToString("000"));
        }
    }

    string CompileAndRead(string code, string fileName, bool enableNullableTypes = true)
    {
        var outputDir = Path.GetTempPath();
        var optionProvider = new TypeScriptOptionProvider();

        optionProvider["build_property.MemoryPackGenerator_TypeScriptOutputDirectory"] = outputDir;
        optionProvider["build_property.MemoryPackGenerator_TypeScriptEnableNullableTypes"] = enableNullableTypes ? "true" : "false";

        CSharpGeneratorRunner.RunGenerator(code, options: optionProvider);

        var outputFilePath = Path.Combine(outputDir, fileName);

        return File.ReadAllText(outputFilePath);
    }

    [Fact]
    public void MEMPACK027_GenerateTypeScriptMustBeMemoryPackable()
    {
        Compile2(27, """
using MemoryPack;

[GenerateTypeScript]
public class Hoge
{
}
""");
    }

    [Fact]
    public void MEMPACK028_GenerateTypeScriptOnlyAllowsGenerateTypeObject()
    {
        Compile2(28, """
using MemoryPack;

[MemoryPackable(GenerateType.Collection)]
[GenerateTypeScript]
public partial class Hoge : System.Collections.Generic.List<int>
{
}
""");
    }

    [Fact]
    public void MEMPACK029_GenerateTypeScriptDoesNotAllowGenerics()
    {
        Compile2(29, """
using MemoryPack;

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge<T>
{
}
""", true);
    }

    [Fact]
    public void MEMPACK030_GenerateTypeScriptDoesNotAllowLongEnum()
    {
        Compile2(30, """
using MemoryPack;

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge
{
    public LongEnum MyProperty { get; set; }
}

public enum LongEnum : long
{

}
""", true);

        Compile2(30, """
using MemoryPack;

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge
{
    public LongEnum[] MyProperty { get; set; }
}

public enum LongEnum : ulong
{

}
""", true);

        Compile2(30, """
using MemoryPack;

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge
{
    public LongEnum? MyProperty { get; set; }
}

public enum LongEnum : long
{

}
""", true);

        Compile2(30, """
using MemoryPack;

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge
{
    public System.Collections.Generic.List<LongEnum> MyProperty { get; set; }
}

public enum LongEnum : long
{

}
""", true);
    }

    [Fact]
    public void MEMPACK031_GenerateTypeScriptNotSupportedType()
    {
        Compile2(31, """
using MemoryPack;

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge
{
    public System.Version MyProperty { get; set; }
}
""", true);
    }

    [Fact]
    public void MEMPACK032_GenerateTypeScriptNotSupportedType()
    {
        Compile2(32, """
using MemoryPack;

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge
{
    [Utf8StringFormatter]
    public string MyProperty { get; set; }
}
""", true);
    }

    [Fact]
    public void GenerateTypeScriptManagedStruct()
    {
        var generatedCode = CompileAndRead("""
            using MemoryPack;

            [MemoryPackable]
            [GenerateTypeScript]
            public partial struct ManagedStruct
            {
                public string Name { get; set; }
                public int Value { get; set; }
            }
            """,
            "ManagedStruct.ts");


        generatedCode.Should().Contain("static serialize(value: ManagedStruct): Uint8Array");
        generatedCode.Should().Contain("static serializeCore(writer: MemoryPackWriter, value: ManagedStruct): void");
        generatedCode.Should().Contain("static deserialize(buffer: ArrayBuffer): ManagedStruct {");
        generatedCode.Should().Contain("static deserializeCore(reader: MemoryPackReader): ManagedStruct {");

        generatedCode.Should().NotContain("writeNullObjectHeader");
        generatedCode.Should().Contain("writer.writeObjectHeader(2)");

        generatedCode.Should().Contain("throw new Error(\"Cannot deserialize null into struct ManagedStruct.\")");
        generatedCode.Should().NotContain("return null");
    }

    [Fact]
    public void GenerateTypeScriptUnmanagedStruct()
    {
        var generatedCode = CompileAndRead("""
            using MemoryPack;

            [MemoryPackable]
            [GenerateTypeScript]
            public partial struct Point
            {
                public int X { get; set; }
                public int Y { get; set; }
            }
            """,
            "Point.ts");

        generatedCode.Should().Contain("static serialize(value: Point): Uint8Array");
        generatedCode.Should().Contain("static serializeCore(writer: MemoryPackWriter, value: Point): void");
        generatedCode.Should().Contain("static deserializeCore(reader: MemoryPackReader): Point {");

        generatedCode.Should().Contain("writer.writeInt32(value.x)");
        generatedCode.Should().Contain("writer.writeInt32(value.y)");
        generatedCode.Should().NotContain("writeObjectHeader");
        generatedCode.Should().NotContain("writeNullObjectHeader");

        generatedCode.Should().Contain("value.x = reader.readInt32()");
        generatedCode.Should().Contain("value.y = reader.readInt32()");
        generatedCode.Should().NotContain("tryReadObjectHeader");
    }

    [Fact]
    public void GenerateTypeScriptUnmanagedStructWithPadding()
    {
        var generatedCode = CompileAndRead("""
            using MemoryPack;

            [MemoryPackable]
            [GenerateTypeScript]
            public partial struct Padded
            {
                public byte A { get; set; }
                public int B { get; set; }
            }
            """,
            "Padded.ts");

        generatedCode.Should().Contain("writer.writeUint8(value.a)");
        generatedCode.Should().Contain("writer.writeZeros(3)");
        generatedCode.Should().Contain("writer.writeInt32(value.b)");
        generatedCode.Should().Contain("value.a = reader.readUint8()");
        generatedCode.Should().Contain("reader.skipBytes(3)");
        generatedCode.Should().Contain("value.b = reader.readInt32()");
    }

    [Fact]
    public void GenerateTypeScriptStructMemberIsNonNullable()
    {
        var generatedCode = CompileAndRead("""
            using MemoryPack;

            [MemoryPackable]
            [GenerateTypeScript]
            public partial struct Point
            {
                public int X { get; set; }
                public int Y { get; set; }
            }

            [MemoryPackable]
            [GenerateTypeScript]
            public partial class Container
            {
                public Point Position { get; set; }
                public string Name { get; set; }
            }
            """,
            "Container.ts");

        generatedCode.Should().Contain("position: Point;");
        generatedCode.Should().Contain("this.position = new Point();");
        generatedCode.Should().NotContain("position: Point | null");
    }

    [Fact]
    public void GenerateTypeScriptNullableReferenceTypes()
    {
        var generatedCode = CompileAndRead(
            """
            using MemoryPack;

            [MemoryPackable]
            [GenerateTypeScript]
            public partial record FullName
            {
                public string FirstName { get; init; }
                public string? MiddleName { get; init; }
                public string LastName { get; init; }
            }
            """,
            "FullName.ts");

        generatedCode.Should().Contain(
            """
            export class FullName {
                firstName: string;
                middleName: string | null;
                lastName: string;

                constructor() {
                    this.firstName = "";
                    this.middleName = null;
                    this.lastName = "";
            """);
    }


    class TypeScriptOptionProvider : AnalyzerConfigOptionsProvider
    {
        readonly Dictionary<string, string> _values = new();

        public string this[string key]
        {
            get => _values[key];
            set => _values[key] = value;
        }

        public override AnalyzerConfigOptions GlobalOptions => new SimpleOptions(_values);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return new SimpleOptions(_values);
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return new SimpleOptions(_values);
        }

        public class SimpleOptions : AnalyzerConfigOptions
        {
            readonly Dictionary<string, string> _configValues;

            public SimpleOptions(Dictionary<string, string> configValues)
            {
                _configValues = configValues;
            }

            /// <inheritdoc />
            public override IEnumerable<string> Keys => _configValues.Keys;

            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            {
                if (_configValues.ContainsKey(key))
                {
                    value = _configValues[key];

                    return true;
                }

                if (key == "build_property.MemoryPackGenerator_TypeScriptOutputDirectory")
                {
                    value = Path.GetTempPath();
                    return true;
                }

                value = null;
                return false;
            }
        }
    }

    [Fact]
    public void MEMPACK043_GenerateTypeScriptDoesNotAllowNullableStruct()
    {
        Compile2(43, """
using MemoryPack;

[MemoryPackable]
[GenerateTypeScript]
public partial struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge
{
    public Point? Position { get; set; }
}
""");
    }
}

[MemoryPackable]
[GenerateTypeScript]
public partial class Hoge
{
    public LongEnum? MyProperty { get; set; }
}

public enum LongEnum : long
{

}

public enum ULongEnum : ulong
{

}

#endif
