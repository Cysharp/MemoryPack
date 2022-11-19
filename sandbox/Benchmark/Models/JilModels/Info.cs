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
    public enum SiteState
    {
        normal,
        closed_beta,
        open_beta,
        linked_meta
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Styling : IGenericEquality<Styling>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string link_color { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string tag_foreground_color { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public string tag_background_color { get; set; }

        public bool Equals(Styling obj)
        {
            return
                this.link_color.TrueEqualsString(obj.link_color) &&
                this.tag_background_color.TrueEqualsString(obj.tag_background_color) &&
                this.tag_foreground_color.TrueEqualsString(obj.tag_foreground_color);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.link_color.TrueEqualsString((string)obj.link_color) &&
                this.tag_background_color.TrueEqualsString((string)obj.tag_background_color) &&
                this.tag_foreground_color.TrueEqualsString((string)obj.tag_foreground_color);
        }
    }


    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Site : IGenericEquality<Site>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string site_type { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string name { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public string logo_url { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public string api_site_parameter { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public string site_url { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public string audience { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public string icon_url { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public List<string> aliases { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public SiteState? site_state { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public Styling styling { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public DateTime? closed_beta_date { get; set; }
        [ProtoMember(12)]
        [Key(11), Id(11)]
        public DateTime? open_beta_date { get; set; }
        [ProtoMember(13)]
        [Key(12), Id(12)]
        public DateTime? launch_date { get; set; }
        [ProtoMember(14)]
        [Key(13), Id(13)]
        public string favicon_url { get; set; }
        [ProtoMember(15)]
        [Key(14), Id(14)]
        public List<RelatedSite> related_sites { get; set; }
        [ProtoMember(16)]
        [Key(15), Id(15)]
        public string twitter_account { get; set; }
        [ProtoMember(17)]
        [Key(16), Id(16)]
        public List<string> markdown_extensions { get; set; }
        [ProtoMember(18)]
        [Key(17), Id(17)]
        public string high_resolution_icon_url { get; set; }

        public bool Equals(Site obj)
        {
            return
                this.aliases.TrueEqualsString(obj.aliases) &&
                this.api_site_parameter.TrueEqualsString(obj.api_site_parameter) &&
                this.audience.TrueEqualsString(obj.audience) &&
                this.closed_beta_date.TrueEquals(obj.closed_beta_date) &&
                this.favicon_url.TrueEqualsString(obj.favicon_url) &&
                this.high_resolution_icon_url.TrueEqualsString(obj.high_resolution_icon_url) &&
                this.icon_url.TrueEqualsString(obj.icon_url) &&
                this.launch_date.TrueEquals(obj.launch_date) &&
                this.logo_url.TrueEqualsString(obj.logo_url) &&
                this.markdown_extensions.TrueEqualsString(obj.markdown_extensions) &&
                this.name.TrueEqualsString(obj.name) &&
                this.open_beta_date.TrueEquals(obj.open_beta_date) &&
                this.related_sites.TrueEqualsList(obj.related_sites) &&
                this.site_state.TrueEquals(obj.site_state) &&
                this.site_type.TrueEqualsString(obj.site_type) &&
                this.site_url.TrueEqualsString(obj.site_url) &&
                this.styling.TrueEquals(obj.styling) &&
                this.twitter_account.TrueEqualsString(obj.twitter_account);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.aliases.TrueEqualsString((IEnumerable<string>)obj.aliases) &&
                this.api_site_parameter.TrueEqualsString((string)obj.api_site_parameter) &&
                this.audience.TrueEqualsString((string)obj.audience) &&
                this.closed_beta_date.TrueEquals((DateTime?)obj.closed_beta_date) &&
                this.favicon_url.TrueEqualsString((string)obj.favicon_url) &&
                this.high_resolution_icon_url.TrueEqualsString((string)obj.high_resolution_icon_url) &&
                this.icon_url.TrueEqualsString((string)obj.icon_url) &&
                this.launch_date.TrueEquals((DateTime?)obj.launch_date) &&
                this.logo_url.TrueEqualsString((string)obj.logo_url) &&
                this.markdown_extensions.TrueEqualsString((IEnumerable<string>)obj.markdown_extensions) &&
                this.name.TrueEqualsString((string)obj.name) &&
                this.open_beta_date.TrueEquals((DateTime?)obj.open_beta_date) &&
                this.related_sites.TrueEqualsListDynamic((IEnumerable<dynamic>)obj.related_sites) &&
                this.site_state.TrueEquals((SiteState?)obj.site_state) &&
                this.site_type.TrueEqualsString((string)obj.site_type) &&
                this.site_url.TrueEqualsString((string)obj.site_url) &&
                (this.styling == null && obj.styling == null || this.styling.EqualsDynamic(obj.styling)) &&
                this.twitter_account.TrueEqualsString((string)obj.twitter_account);
        }
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class Info : IGenericEquality<Info>
    {


        [ProtoMember(1)]
        [Key(0), Id(0)]
        public int? total_questions { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public int? total_unanswered { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public int? total_accepted { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public int? total_answers { get; set; }
        [ProtoMember(5)]
        [Key(4), Id(4)]
        public decimal? questions_per_minute { get; set; }
        [ProtoMember(6)]
        [Key(5), Id(5)]
        public decimal? answers_per_minute { get; set; }
        [ProtoMember(7)]
        [Key(6), Id(6)]
        public int? total_comments { get; set; }
        [ProtoMember(8)]
        [Key(7), Id(7)]
        public int? total_votes { get; set; }
        [ProtoMember(9)]
        [Key(8), Id(8)]
        public int? total_badges { get; set; }
        [ProtoMember(10)]
        [Key(9), Id(9)]
        public decimal? badges_per_minute { get; set; }
        [ProtoMember(11)]
        [Key(10), Id(10)]
        public int? total_users { get; set; }
        [ProtoMember(12)]
        [Key(11), Id(11)]
        public int? new_active_users { get; set; }
        [ProtoMember(13)]
        [Key(12), Id(12)]
        public string api_revision { get; set; }
        [ProtoMember(14)]
        [Key(13), Id(13)]
        public Site site { get; set; }

        public bool Equals(Info obj)
        {
            return
                this.answers_per_minute.TrueEquals(obj.answers_per_minute) &&
                this.api_revision.TrueEqualsString(obj.api_revision) &&
                this.badges_per_minute.TrueEquals(obj.badges_per_minute) &&
                this.new_active_users.TrueEquals(obj.new_active_users) &&
                this.questions_per_minute.TrueEquals(obj.questions_per_minute) &&
                this.site.TrueEquals(obj.site) &&
                this.total_accepted.TrueEquals(obj.total_accepted) &&
                this.total_answers.TrueEquals(obj.total_answers) &&
                this.total_badges.TrueEquals(obj.total_badges) &&
                this.total_comments.TrueEquals(obj.total_comments) &&
                this.total_questions.TrueEquals(obj.total_questions) &&
                this.total_unanswered.TrueEquals(obj.total_unanswered) &&
                this.total_users.TrueEquals(obj.total_users) &&
                this.total_votes.TrueEquals(obj.total_votes);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.answers_per_minute.TrueEquals((decimal?)obj.answers_per_minute) &&
                this.api_revision.TrueEqualsString((string)obj.api_revision) &&
                this.badges_per_minute.TrueEquals((decimal?)obj.badges_per_minute) &&
                this.new_active_users.TrueEquals((int?)obj.new_active_users) &&
                this.questions_per_minute.TrueEquals((decimal?)obj.questions_per_minute) &&
                (this.site == null && obj.site == null || this.site.EqualsDynamic(obj.site)) &&
                this.total_accepted.TrueEquals((int?)obj.total_accepted) &&
                this.total_answers.TrueEquals((int?)obj.total_answers) &&
                this.total_badges.TrueEquals((int?)obj.total_badges) &&
                this.total_comments.TrueEquals((int?)obj.total_comments) &&
                this.total_questions.TrueEquals((int?)obj.total_questions) &&
                this.total_unanswered.TrueEquals((int?)obj.total_unanswered) &&
                this.total_users.TrueEquals((int?)obj.total_users) &&
                this.total_votes.TrueEquals((int?)obj.total_votes);
        }
    }

    public enum SiteRelation
    {
        parent,
        meta,
        chat
    }

    [ProtoContract]
    [MemoryPackable, MessagePackObject, GenerateSerializer]
    public partial class RelatedSite : IGenericEquality<RelatedSite>
    {
        [ProtoMember(1)]
        [Key(0), Id(0)]
        public string name { get; set; }
        [ProtoMember(2)]
        [Key(1), Id(1)]
        public string site_url { get; set; }
        [ProtoMember(3)]
        [Key(2), Id(2)]
        public SiteRelation? relation { get; set; }
        [ProtoMember(4)]
        [Key(3), Id(3)]
        public string api_site_parameter { get; set; }

        public bool Equals(RelatedSite obj)
        {
            return
                this.name.TrueEqualsString(obj.name) &&
                this.relation.TrueEquals(obj.relation) &&
                this.api_site_parameter.TrueEqualsString(obj.api_site_parameter);
        }

        public bool EqualsDynamic(dynamic obj)
        {
            return
                this.name.TrueEqualsString((string)obj.name) &&
                this.relation.TrueEquals((SiteRelation?)obj.relation) &&
                this.api_site_parameter.TrueEqualsString((string)obj.api_site_parameter);
        }
    }
}
