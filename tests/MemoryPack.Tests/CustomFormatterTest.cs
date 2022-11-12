#pragma warning disable CS8602

using MemoryPack.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class CustomFormatterTest
{
    [Fact]
    public void Check()
    {
        var value = new CustomFormatterCheck()
        {
            NoMarkField = "aaaa",
            Field1 = "aaaa",
            Prop1 = "bbbb",
            NoMarkProp = "bbbb",
            PropDict = new Dictionary<string, int> { { "ZooM", 999 }, { "DdddN", 10000 } },
            FieldDict = new Dictionary<string, string> { { "hOGe", "hugahuga" }, { "HagE", "nanonano" } },
        };


        var bin1 = MemoryPackSerializer.Serialize(value, MemoryPackSerializeOptions.Utf8);
        var bin2 = MemoryPackSerializer.Serialize(value, MemoryPackSerializeOptions.Utf16);

        var v1 = MemoryPackSerializer.Deserialize<CustomFormatterCheck>(bin1);
        var v2 = MemoryPackSerializer.Deserialize<CustomFormatterCheck>(bin2);

#if NET7_0_OR_GREATER
        v1.PropDict["zoom"].Should().Be(999);
        v1.PropDict["DDDDN"].Should().Be(10000);
        v1.FieldDict["HOGE"].Should().Be("hugahuga");
        v1.FieldDict["hage"].Should().Be("nanonano");
#endif

        v1.Prop1.Should().Be(value.Prop1);
        v1.Field1.Should().Be(value.Field1);
        v2.Prop1.Should().Be(value.Prop1);
        v2.Field1.Should().Be(value.Field1);
    }
}
