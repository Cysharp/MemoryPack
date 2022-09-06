using Microsoft.CodeAnalysis;

namespace MemoryPack.Generator
{
    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.RegularExpressions/gen/RegexGenerator.cs
    // https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
    // https://andrewlock.net/creating-a-source-generator-part-1-creating-an-incremental-source-generator/
    // https://qiita.com/WiZLite/items/48f37278cf13be899e40
    // https://zenn.dev/pcysl5edgo/articles/6d9be0dd99c008
    // https://neue.cc/2021/05/08_600.html
    // https://www.thinktecture.com/en/net/roslyn-source-generators-introduction/

    [Generator(LanguageNames.CSharp)]
    public partial class MemoryPackGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var sw = new StringWriter();




            // context.AddSource("RegexGenerator.g.cs", sw.ToString());
        }
    }
}
