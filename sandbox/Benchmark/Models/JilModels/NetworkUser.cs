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
    public partial class NetworkUser : IGenericEquality<NetworkUser>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string site_name { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string site_url { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? user_id { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public int? reputation { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public int? account_id { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public UserType? user_type { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public BadgeCount badge_counts { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public DateTime? last_access_date { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public int? answer_count { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public int? question_count { get; set; }

        public bool Equals(NetworkUser obj)
        {
            return
                this.account_id.TrueEquals(obj.account_id) &&
                this.answer_count.TrueEquals(obj.answer_count) &&
                this.badge_counts.TrueEquals(obj.badge_counts) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.last_access_date.TrueEquals(obj.last_access_date) &&
                this.question_count.TrueEquals(obj.question_count) &&
                this.reputation.TrueEquals(obj.reputation) &&
                this.site_name.TrueEqualsString(obj.site_name) &&
                this.site_url.TrueEqualsString(obj.site_url) &&
                this.user_id.TrueEquals(obj.user_id) &&
                this.user_type.TrueEquals(obj.user_type);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.account_id.TrueEquals((int?)obj.account_id) &&
                this.answer_count.TrueEquals((int?)obj.answer_count) &&
                (this.badge_counts == null && obj.badge_counts == null || this.badge_counts.EqualsDynamic(obj.badge_counts)) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.last_access_date.TrueEquals((DateTime?)obj.last_access_date) &&
                this.question_count.TrueEquals((int?)obj.question_count) &&
                this.reputation.TrueEquals((int?)obj.reputation) &&
                this.site_name.TrueEqualsString((string)obj.site_name) &&
                this.site_url.TrueEqualsString((string)obj.site_url) &&
                this.user_id.TrueEquals((int?)obj.user_id) &&
                this.user_type.TrueEquals((UserType?)obj.user_type);
        }
    }
}
