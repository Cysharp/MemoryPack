using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.SourceGeneratorTests;

public class IncrementalGeneratorTest
{
    [Fact]
    public void Run()
    {
        // lang=C#-test
        var step1 = """
[MemoryPackable]
public partial class MyClass
{
    public int MyProperty { get; set; }
}
""";

        // lang=C#-test
        var step2 = """
[MemoryPackable]
public partial class MyClass
{
    public int MyProperty { get; set; }
    // unrelated line
}
""";

        var hoge = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("MemoryPack.MemoryPackable.", step1, step2);

    }

    [Fact]
    public void SerializerContextGraphIsCachedForUnchangedCompilation()
    {
        var source = """
[MemoryPackable]
public partial class ContextDto
{
    public int Id { get; set; }
}

[MemoryPackSerializable(typeof(ContextDto))]
public partial class GeneratedContext : MemoryPackSerializerContext
{
}
""";

        var reasons = CSharpGeneratorRunner.GetCachedIncrementalGeneratorTrackedStepsReasons(
            "MemoryPack.SerializerContext.",
            source,
            "NET7_0_OR_GREATER",
            "NET8_0_OR_GREATER");

        reasons.Should().NotBeEmpty();
        reasons.Select(x => x.Reasons).Should().OnlyContain(x => !x.Contains("New", StringComparison.Ordinal));
    }
}

