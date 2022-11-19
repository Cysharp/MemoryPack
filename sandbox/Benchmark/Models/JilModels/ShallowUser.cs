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
    public enum UserType : byte
    {
        unregistered = 2,
        registered = 3,
        moderator = 4,
        does_not_exist = 255
    }
    
    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class ShallowUser : IGenericEquality<ShallowUser>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? user_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string display_name { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? reputation { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public UserType? user_type { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public string profile_image { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public string link { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public int? accept_rate { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public BadgeCount badge_counts { get; set; }

        public bool Equals(ShallowUser obj)
        {
            return
                this.accept_rate.TrueEquals(obj.accept_rate) &&
                this.badge_counts.TrueEquals(obj.badge_counts) &&
                this.display_name.TrueEqualsString(obj.display_name) &&
                this.link.TrueEqualsString(obj.link) &&
                this.profile_image.TrueEqualsString(obj.profile_image) &&
                this.reputation.TrueEquals(obj.reputation) &&
                this.user_id.TrueEquals(obj.user_id) &&
                this.user_type.TrueEquals(obj.user_type);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.accept_rate.TrueEquals((int?)obj.accept_rate) &&
                (this.badge_counts == null && obj.badge_counts == null || this.badge_counts.EqualsDynamic(obj.badge_counts)) &&
                this.display_name.TrueEqualsString((string)obj.display_name) &&
                this.link.TrueEqualsString((string)obj.link) &&
                this.profile_image.TrueEqualsString((string)obj.profile_image) &&
                this.reputation.TrueEquals((int?)obj.reputation) &&
                this.user_id.TrueEquals((int?)obj.user_id) &&
                this.user_type.TrueEquals((UserType?)obj.user_type);
        }
    }
}
