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
    public partial class Error : IGenericEquality<Error>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? error_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string error_name { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public string description { get; set; }

        public bool Equals(Error obj)
        {
            return
                this.error_id.TrueEquals(obj.error_id) &&
                this.error_name.TrueEqualsString(obj.error_name) &&
                this.description.TrueEqualsString(obj.description);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.error_id.TrueEquals((int?)obj.error_id) &&
                this.error_name.TrueEqualsString((string)obj.error_name) &&
                this.description.TrueEqualsString((string)obj.description);
        }
    }
}
