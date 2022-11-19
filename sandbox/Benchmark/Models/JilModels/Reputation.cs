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
    public enum VoteType : byte
    {
        up_votes = 2,
        down_votes = 3,
        spam = 12,
        accepts = 1,
        bounties_won = 9,
        bounties_offered = 8,
        suggested_edits = 16
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Reputation : IGenericEquality<Reputation>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? user_id { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? post_id { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public PostType? post_type { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public VoteType? vote_type { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public string title { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public string link { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public int? reputation_change { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public DateTime? on_date { get; set; }

        public bool Equals(Reputation obj)
        {
            return
                this.link.TrueEqualsString(obj.link) &&
                this.on_date.TrueEquals(obj.on_date) &&
                this.post_id.TrueEquals(obj.post_id) &&
                this.post_type.TrueEquals(obj.post_type) &&
                this.reputation_change.TrueEquals(obj.reputation_change) &&
                this.title.TrueEqualsString(obj.title) &&
                this.user_id.TrueEquals(obj.user_id) &&
                this.vote_type.TrueEquals(obj.vote_type);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.link.TrueEqualsString((string)obj.link) &&
                this.on_date.TrueEquals((DateTime?)obj.on_date) &&
                this.post_id.TrueEquals((int?)obj.post_id) &&
                this.post_type.TrueEquals((PostType?)obj.post_type) &&
                this.reputation_change.TrueEquals((int?)obj.reputation_change) &&
                this.title.TrueEqualsString((string)obj.title) &&
                this.user_id.TrueEquals((int?)obj.user_id) &&
                this.vote_type.TrueEquals((VoteType?)obj.vote_type);
        }
    }
}
