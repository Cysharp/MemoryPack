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
        var diagnostics = CSharpGeneratorRunner.RunGenerator(code, new TypeScriptOptionProvider());
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


    class TypeScriptOptionProvider : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions => new SimpleOptions();

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return new SimpleOptions();
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return new SimpleOptions();
        }

        class SimpleOptions : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            {
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
