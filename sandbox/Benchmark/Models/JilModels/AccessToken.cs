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
    public partial class AccessToken : IGenericEquality<AccessToken>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string access_token { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public DateTime? expires_on_date { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? account_id { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public List<string> scope { get; set; }

        public bool Equals(AccessToken obj)
        {
            return
                access_token.TrueEqualsString(obj.access_token) ||
                expires_on_date.TrueEquals(obj.expires_on_date) ||
                account_id.TrueEquals(obj.account_id) ||
                scope.TrueEqualsString(obj.scope);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                access_token.TrueEqualsString((string)obj.access_token) ||
                expires_on_date.TrueEquals((DateTime?)obj.expires_on_date) ||
                account_id.TrueEquals((int?)obj.account_id) ||
                scope.TrueEqualsString((IEnumerable<string>)obj.scope);
        }
    }
}
