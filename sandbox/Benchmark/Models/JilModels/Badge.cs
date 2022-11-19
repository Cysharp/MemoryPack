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
    public enum BadgeRank : byte
    {
        bronze = 3,
        silver = 2,
        gold = 1
    }

    public enum BadgeType
    {
        named = 1,
        tag_based = 2
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Badge : IGenericEquality<Badge>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? badge_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public BadgeRank? rank { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public string name { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public string description { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public int? award_count { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public BadgeType? badge_type { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public ShallowUser user { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public string link { get; set; }

        public bool Equals(Badge obj)
        {
            return
                this.award_count.TrueEquals(obj.award_count) &&
                this.badge_id.TrueEquals(obj.badge_id) &&
                this.badge_type.TrueEquals(obj.badge_type) &&
                this.description.TrueEqualsString(obj.description) &&
                this.link.TrueEqualsString(obj.link) &&
                this.name.TrueEqualsString(obj.name) &&
                this.rank.TrueEquals(obj.rank) &&
                this.user.TrueEquals(obj.user);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.award_count.TrueEquals((int?)obj.award_count) &&
                this.badge_id.TrueEquals((int?)obj.badge_id) &&
                this.badge_type.TrueEquals((BadgeType?)obj.badge_type) &&
                this.description.TrueEqualsString((string)obj.description) &&
                this.link.TrueEqualsString((string)obj.link) &&
                this.name.TrueEqualsString((string)obj.name) &&
                this.rank.TrueEquals((BadgeRank?)obj.rank) &&
                (this.user == null && obj.user == null || this.user.EqualsDynamic(obj.user));
        }
    }
}
