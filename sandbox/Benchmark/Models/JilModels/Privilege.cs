#nullable disable
using MemoryPack;
using MessagePack;
using Orleans;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Models
{
    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Privilege : IGenericEquality<Privilege>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string short_description { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string description { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? reputation { get; set; }

        public bool Equals(Privilege obj)
        {
            return
                this.description.TrueEqualsString(obj.description) &&
                this.reputation.TrueEquals(obj.reputation) &&
                this.short_description.TrueEqualsString(obj.short_description);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.description.TrueEqualsString((string)obj.description) &&
                this.reputation.TrueEquals((int?)obj.reputation) &&
                this.short_description.TrueEqualsString((string)obj.short_description);
        }
    }
}
