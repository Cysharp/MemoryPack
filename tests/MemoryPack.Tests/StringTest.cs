using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Tests;

public class StringTest
{
    [Fact]
    public void Utf16()
    {
        var text = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほわをん";

        var bin = MemoryPackSerializer.Serialize(text, MemoryPackSerializeOptions.Utf16);
        var newText = MemoryPackSerializer.Deserialize<string>(bin);

        text.Should().Be(newText);
    }

    [Fact]
    public void Utf8()
    {
        var text = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほわをん";

        var bin = MemoryPackSerializer.Serialize(text, MemoryPackSerializeOptions.Utf8);
        var newText = MemoryPackSerializer.Deserialize<string>(bin);

        text.Should().Be(newText);
    }

    [Fact]
    public void MalformedUtf8()
    {
        var text = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほわをん";

        var bin = MemoryPackSerializer.Serialize(text, MemoryPackSerializeOptions.Utf8);

        ref var head = ref MemoryMarshal.GetArrayDataReference(bin);

        // [utf8-length, utf16-length, utf8-value]
        // change utf16-length

        Unsafe.WriteUnaligned(ref Unsafe.Add(ref head, 4), 9999);

        Assert.Throws<MemoryPackSerializationException>(()=> MemoryPackSerializer.Deserialize<string>(bin));
    }
}
