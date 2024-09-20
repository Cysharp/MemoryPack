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
}


