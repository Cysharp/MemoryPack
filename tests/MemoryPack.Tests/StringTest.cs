using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Tests;

public class StringTest
{
    [Fact]
    public void Utf16()
    {
        var text = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほわをん";

        var bin = MemoryPackSerializer.Serialize(text, MemoryPackSerializerOptions.Utf16);
        var newText = MemoryPackSerializer.Deserialize<string>(bin);

        text.Should().Be(newText);
    }

    [Fact]
    public void Utf8()
    {
        var text = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほわをん";

        var bin = MemoryPackSerializer.Serialize(text, MemoryPackSerializerOptions.Utf8);
        var newText = MemoryPackSerializer.Deserialize<string>(bin);

        text.Should().Be(newText);
    }

    [Fact]
    public void MalformedUtf8()
    {
        var text = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほわをん";

        var bin = MemoryPackSerializer.Serialize(text, MemoryPackSerializerOptions.Utf8);

        ref var head = ref MemoryMarshal.GetArrayDataReference(bin);

        // (int ~utf8-byte-count, int utf16-length, utf8-bytes)
        // change utf16-length

        Unsafe.WriteUnaligned(ref Unsafe.Add(ref head, 4), 9999);

        Assert.Throws<MemoryPackSerializationException>(() => MemoryPackSerializer.Deserialize<string>(bin));
    }

    [Fact]
    public void Intern()
    {
        var bin = MemoryPackSerializer.Serialize(Guid.NewGuid().ToString());

        var str1 = MemoryPackSerializer.Deserialize<string>(bin);
        var str2 = MemoryPackSerializer.Deserialize<string>(bin);

        str1.Should().Be(str2);
        object.ReferenceEquals(str1, str2).Should().BeFalse();

        var value = new InternStringTest { Foo = Guid.NewGuid().ToString() };

        var bin2 = MemoryPackSerializer.Serialize(value);

        var v1 = MemoryPackSerializer.Deserialize<InternStringTest>(bin2)!;
        var v2 = MemoryPackSerializer.Deserialize<InternStringTest>(bin2)!;

        v1.Foo.Should().Be(v2.Foo);
        object.ReferenceEquals(v1.Foo, v2.Foo).Should().BeTrue();

        string.IsInterned(v1.Foo!).Should().NotBeNull();
    }
}


[MemoryPackable]
public partial class InternStringTest
{
    [InternStringFormatter]
    public string? Foo { get; set; }
}
