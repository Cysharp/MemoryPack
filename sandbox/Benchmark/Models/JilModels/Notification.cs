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
    public enum NotificationType : byte
    {
        generic = 1,
        accounts_associated = 8,
        badge_earned = 5,
        profile_activity = 2,
        bounty_expired = 3,
        bounty_expires_in_one_day = 4,
        bounty_expires_in_three_days = 6,
        edit_suggested = 22,
        new_privilege = 9,
        post_migrated = 10,
        moderator_message = 11,
        registration_reminder = 12,
        substantive_edit = 23,
        reputation_bonus = 7,
        bounty_grace_period_started = 24
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Notification : IGenericEquality<Notification>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public NotificationType? notification_type { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public Site site { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public string body { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public int? post_id { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public bool? is_unread { get; set; }

        public bool Equals(Notification obj)
        {
            return
                this.body.TrueEqualsString(obj.body) &&
                this.site.TrueEquals(obj.site) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.post_id.TrueEquals(obj.post_id) &&
                this.is_unread.TrueEquals(obj.is_unread);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.body.TrueEqualsString((string)obj.body) &&
                (this.site == null && obj.site == null || this.site.EqualsDynamic(obj.site)) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.post_id.TrueEquals((int?)obj.post_id) &&
                this.is_unread.TrueEquals((bool?)obj.is_unread);
        }
    }
}
