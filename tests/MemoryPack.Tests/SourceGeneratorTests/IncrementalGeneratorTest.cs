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
            
            [MemoryPackable]
            public partial class CollectionTest
            {
                public Collection<string> Collection { get; } = new Collection<string>();

                [MemoryPackOnSerializing]
                void OnSerializing2()
                {
                    Console.WriteLine(nameof(OnSerializing2));
                }
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


