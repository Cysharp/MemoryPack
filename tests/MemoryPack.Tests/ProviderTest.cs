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
        Assert.Throws<MemoryPackSerializationException>(() =>
        MemoryPackSerializer.Serialize(new { foo = 10, bar = "tako" })).Message.Should().Contain("anonymous type");
    }

    [Fact]

    public void RegisterAs()
    {
        MemoryPackFormatterProvider.RegisterGenericType(typeof(CustomType<>), typeof(CustomTypeFormatter<>));

        var t = new CustomType<int>() { Value = 9999 };
        var bin = MemoryPackSerializer.Serialize(t);
        MemoryPackSerializer.Deserialize<CustomType<int>>(bin)!.Value.Should().Be(9999);
    }

    //[Fact]
    //public void RegisterGenericTest()
    //{
    //    MemoryPackFormatterProvider.RegisterCollection<MyList, int>();

    //    var list = new MyList { 1, 10, 100 };


    //    var bin = MemoryPackSerializer.Serialize(list);
    //    var foo = MemoryPackSerializer.Deserialize<MyList>(bin);
    //}

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
        writer.WriteValue<T>(value.Value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref CustomType<T?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }
        if (count != 1) MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);

        value = new CustomType<T?> { Value = reader.ReadValue<T>() };
    }
}

public class MyList : List<int>
{
}


