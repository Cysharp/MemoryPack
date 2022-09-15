using MessagePack;
using Orleans;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Models;

[MessagePackObject]
[ProtoContract]
// [GenerateSerializer]
public struct Vector3
{
    [Key(0)]
    [ProtoBuf.ProtoMember(1)]
    [Id(0)]
    public float X;
    [Key(1)]
    [ProtoBuf.ProtoMember(2)]
    [Id(1)]
    public float Y;
    [Key(2)]
    [ProtoBuf.ProtoMember(3)]
    [Id(2)]
    public float Z;
}
