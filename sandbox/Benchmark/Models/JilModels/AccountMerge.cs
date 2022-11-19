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
    public partial class AccountMerge : IGenericEquality<AccountMerge>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? old_account_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? new_account_id { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public DateTime? merge_date { get; set; }

        public bool Equals(AccountMerge obj)
        {
            return
                old_account_id.TrueEquals(obj.old_account_id) &&
                new_account_id.TrueEquals(obj.new_account_id) &&
                merge_date.TrueEquals(obj.merge_date);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                old_account_id.TrueEquals((int?)obj.old_account_id) &&
                new_account_id.TrueEquals((int?)obj.new_account_id) &&
                merge_date.TrueEquals((DateTime?)obj.merge_date);
        }
    }
}
