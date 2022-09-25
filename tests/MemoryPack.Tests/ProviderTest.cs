using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class ProviderTest
{
    [Fact]
    public void AnonymousType()
    {
        Assert.Throws<InvalidOperationException>(() =>
        MemoryPackSerializer.Serialize(new { foo = 10, bar = "tako" })).Message.Should().Contain("anonymous type");
    }

    [Fact]

    public void RegisterAs()
    {
        MemoryPackFormatterProvider.RegisterGeneric(typeof(CustomType<>), typeof(CustomTypeFormatter<>));

        var t = new CustomType<int>() { Value = 9999 };
        var bin = MemoryPackSerializer.Serialize(t);
        MemoryPackSerializer.Deserialize<CustomType<int>>(bin)!.Value.Should().Be(9999);
    }

    // TODO: NonGenerics only API can get formatter?
}

public class CustomType<T>
{
    public T? Value { get; set; }
} 

public class CustomTypeFormatter<T> : MemoryPackFormatter<CustomType<T?>>
    where T : notnull
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref CustomType<T?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteObject<T>(value.Value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref CustomType<T?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }
        if (count != 1) ThrowHelper.ThrowInvalidPropertyCount(1, count);

        value = new CustomType<T?> { Value = reader.ReadObject<T>() };
    }
}
