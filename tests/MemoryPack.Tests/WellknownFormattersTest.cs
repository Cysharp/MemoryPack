using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace MemoryPack.Tests;

public class WellknownFormattersTest
{
    private T Convert<T>(T value)
    {
        return MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value))!;
    }

    private void ConvertEqual<T>(T value)
    {
        Convert(value).Should().Be(value);
    }

    [Fact]
    public void Geenrics()
    {
        ConvertEqual(new KeyValuePair<int, string>(100, "hoge"));
        Convert(new Lazy<int>(100)).Value.Should().Be(100);
    }

    [Fact]
    public void Nullable()
    {
        Convert(new Nullable<Sonota>(new Sonota { MyProperty = "9" })!)!.Value.MyProperty.Should().Be("9");
        Convert(default(Sonota?)).HasValue.Should().BeFalse();
    }


    [Fact]
    public void Others()
    {
        ConvertEqual(new Version(1, 3, 4, 5));
        ConvertEqual(new Uri("http://hoehoge.com/huu?q=takotako"));
        ConvertEqual(TimeZoneInfo.Utc);
        var sb = new StringBuilder(new string('a', 99999));
        Convert(sb).ToString().Should().Be(sb.ToString());
        ConvertEqual(typeof(WellknownFormattersTest));
        var bitArray = new BitArray(Enumerable.Range(1, 1000).Select(x => x % 3 == 0).ToArray());
        Convert(bitArray).OfType<bool>().ToArray().Should().Equal(bitArray.OfType<bool>().ToArray());
        ConvertEqual(CultureInfo.InvariantCulture);
        ConvertEqual(CultureInfo.GetCultureInfo("ja"));
        ConvertEqual(CultureInfo.GetCultureInfo("ja-JP"));
        ConvertEqual(CultureInfo.GetCultureInfo("en"));
    }
}

[MemoryPackable]
public partial struct Sonota
{
    public string MyProperty { get; set; }
}
