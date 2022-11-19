#nullable disable
using MessagePack;
using MemoryPack;
using Orleans;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.Models
{
    public enum UserTimelineType : byte
    {
        commented = 1,
        asked = 2,
        answered = 3,
        badge = 4,
        revision = 5,
        accepted = 6,
        reviewed = 7,
        suggested = 8
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class UserTimeline : IGenericEquality<UserTimeline>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public PostType? post_type { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public UserTimelineType? timeline_type { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public int? user_id { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public int? post_id { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public int? comment_id { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public int? suggested_edit_id { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public int? badge_id { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public string title { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public string detail { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public string link { get; set; }

        public bool Equals(UserTimeline obj)
        {
            return
                this.badge_id.TrueEquals(obj.badge_id) &&
                this.comment_id.TrueEquals(obj.comment_id) &&
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.detail.TrueEqualsString(obj.detail) &&
                this.link.TrueEqualsString(obj.link) &&
                this.post_id.TrueEquals(obj.post_id) &&
                this.post_type.TrueEquals(obj.post_type) &&
                this.suggested_edit_id.TrueEquals(obj.suggested_edit_id) &&
                this.timeline_type.TrueEquals(obj.timeline_type) &&
                this.title.TrueEqualsString(obj.title) &&
                this.user_id.TrueEquals(obj.user_id);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.badge_id.TrueEquals((int?)obj.badge_id) &&
                this.comment_id.TrueEquals((int?)obj.comment_id) &&
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.detail.TrueEqualsString((string)obj.detail) &&
                this.link.TrueEqualsString((string)obj.link) &&
                this.post_id.TrueEquals((int?)obj.post_id) &&
                this.post_type.TrueEquals((PostType?)obj.post_type) &&
                this.suggested_edit_id.TrueEquals((int?)obj.suggested_edit_id) &&
                this.timeline_type.TrueEquals((UserTimelineType?)obj.timeline_type) &&
                this.title.TrueEqualsString((string)obj.title) &&
                this.user_id.TrueEquals((int?)obj.user_id);
        }
    }
}
