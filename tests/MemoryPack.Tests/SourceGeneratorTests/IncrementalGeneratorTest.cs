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

        //TODO: this is just while WIP
        var code = """
            using System.Collections.ObjectModel;
            using MemoryPack;
            
            
            [MemoryPackable(GenerateType.CircularReference)]
            public partial class CircularReferenceWithRequiredProperties
            {
                [MemoryPackOrder(0)]
                public required string FirstName { get; init; }
                [MemoryPackOrder(1)]
                public required string LastName { get; set; }
                [MemoryPackOrder(2)]
                public CircularReferenceWithRequiredProperties? Manager { get; init; }
                [MemoryPackOrder(3)]
                public required List<CircularReferenceWithRequiredProperties> DirectReports { get; set; }
            }
            
            """;
        var k = string.Join(Environment.NewLine, CSharpGeneratorRunner.RunGenerator(code).Item1.SyntaxTrees.Select(x => x.ToString()));

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


