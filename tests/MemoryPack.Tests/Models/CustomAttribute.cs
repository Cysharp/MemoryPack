using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;

[MemoryPackable]
public partial class CustomFormatterCheck
{
    public string? NoMarkField;
    public string? NoMarkProp { get; set; }

    [Utf8StringFormatter]
    public string? Field1;

    [Utf16StringFormatter]
    public string? Prop1 { get; set; }

#if NET7_0_OR_GREATER
    [OrdinalIgnoreCaseStringDictionaryFormatter<int>]
#endif
    public Dictionary<string, int>? PropDict { get; set; }
#if NET7_0_OR_GREATER
    [OrdinalIgnoreCaseStringDictionaryFormatter<string>]
#endif
    public Dictionary<string, string>? FieldDict;

}
