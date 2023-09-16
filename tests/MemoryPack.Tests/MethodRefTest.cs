using System;
using System.Buffers;

namespace MemoryPack.Tests;

public class MethodRefTest
{
    [Fact]
    public void WriteId()
    {
        var data = new EmitIdData { MyProperty = 9999 };
        var bin = MemoryPackSerializer.Serialize(data);

        EmitIdData.privateData = Guid.Empty;
        var v2 = MemoryPackSerializer.Deserialize<EmitIdData>(bin);
        v2!.MyProperty.Should().Be(data.MyProperty);

        EmitIdData.privateData.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ReadOther()
    {
        var data = new EmitFromOther();
        data.Set(9999);

        var reference = new EmitFromOther();
        EmitFromOther.other = reference;

        var bin = MemoryPackSerializer.Serialize(data);


        var v2 = MemoryPackSerializer.Deserialize<EmitFromOther>(bin);
        v2!.MyProperty.Should().Be(data.MyProperty);

        v2!.Should().BeSameAs(reference);
    }
}

[MemoryPackable]
public partial class EmitIdData
{
    public int MyProperty { get; set; }

    public static Guid privateData;

    [MemoryPackOnSerializing]
    static void WriteId<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EmitIdData? value)
#if NET7_0_OR_GREATER
        where TBufferWriter : IBufferWriter<byte>
#else
        where TBufferWriter : class, IBufferWriter<byte>
#endif
    {
        writer.WriteUnmanaged(Guid.NewGuid()); // emit GUID in header.
    }

    [MemoryPackOnDeserializing]
    static void ReadId(ref MemoryPackReader reader, ref EmitIdData? value)
    {
        // read custom header before deserialize
        var guid = reader.ReadUnmanaged<Guid>();
        Console.WriteLine(guid);
        privateData = guid;
    }
}


[MemoryPackable]
public partial class EmitFromOther
{
    public static EmitFromOther other;

    public int MyProperty { get; private set; }

    public void Set(int v)
    {
        MyProperty = v;
    }

    [MemoryPackOnDeserializing]
    static void ReadId(ref MemoryPackReader reader, ref EmitFromOther? value)
    {
        value = other!;
    }
}
