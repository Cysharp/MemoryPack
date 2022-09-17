using MemoryPack;
using MessagePack;
using Orleans;
using ProtoBuf;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Models;

[MessagePackObject]
[ProtoContract]
//[GenerateSerializer]
public partial class MyClass : IMemoryPackable<MyClass>
{
    [Key(0)]
    [ProtoBuf.ProtoMember(1)]
    [Id(0)]
    public int X { get; set; }
    [Key(1)]
    [ProtoBuf.ProtoMember(2)]
    [Id(1)]
    public int Y { get; set; }
    [Key(2)]
    [ProtoBuf.ProtoMember(3)]
    [Id(2)]
    public int Z { get; set; }
    [Key(3)]
    [ProtoBuf.ProtoMember(4)]
    [Id(3)]
    public string? FirstName { get; set; }
    [Key(4)]
    [ProtoBuf.ProtoMember(5)]
    [Id(4)]
    public string? LastName { get; set; }

    static MyClass()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<MyClass>())
        {
            MemoryPackFormatterProvider.Register(new Formatter());
        }
    }

    static void IMemoryPackable<MyClass>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref MyClass? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }
        else
        {
            writer.WriteObjectHeader(4);
        }

        writer.WriteUnmanaged(value.X, value.Y, value.Z);
        writer.WriteString(value.FirstName);
        writer.WriteString(value.LastName);
    }

    static void IMemoryPackable<MyClass>.Deserialize(ref MemoryPackReader reader, scoped ref MyClass? value)
    {
        throw new NotImplementedException();
    }

    class Formatter : MemoryPackFormatter<MyClass>
    {
        public override void Deserialize(ref MemoryPackReader reader, scoped ref MyClass? value)
        {

        }

        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref MyClass? value)
        {
            writer.WritePackable(ref value);
        }
    }
}
