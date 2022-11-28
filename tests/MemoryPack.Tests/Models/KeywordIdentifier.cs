using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;


// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/verbatim

[MemoryPackable]
public partial class KeywordModel
{
    public int @int;
    public long @long;
    public string? @string { get; set; }

    public Version2? @for;

    [MemoryPackConstructor]
    public KeywordModel(int @int)
    {
        this.@int = @int;
    }
}
