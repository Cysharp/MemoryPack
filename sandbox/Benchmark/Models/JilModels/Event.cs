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
    public enum EventType : byte
    {
        question_posted = 1,
        answer_posted = 2,
        comment_posted = 3,
        post_edited = 4,
        user_created = 5
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Event : IGenericEquality<Event>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public EventType? event_type { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? event_id { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public DateTime? creation_date { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public string link { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public string excerpt { get; set; }

        public bool Equals(Event obj)
        {
            return
                this.creation_date.TrueEquals(obj.creation_date) &&
                this.event_id.TrueEquals(obj.event_id) &&
                this.event_type.TrueEquals(obj.event_type) &&
                this.excerpt.TrueEqualsString(obj.excerpt) &&
                this.link.TrueEqualsString(obj.link);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.creation_date.TrueEquals((DateTime?)obj.creation_date) &&
                this.event_id.TrueEquals((int?)obj.event_id) &&
                this.event_type.TrueEquals((EventType?)obj.event_type) &&
                this.excerpt.TrueEqualsString((string)obj.excerpt) &&
                this.link.TrueEqualsString((string)obj.link);
        }
    }
}
