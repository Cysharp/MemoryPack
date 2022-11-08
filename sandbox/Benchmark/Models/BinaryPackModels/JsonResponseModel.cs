using System;
using System.Collections.Generic;
using System.Linq;
using BinaryPack.Models.Helpers;
using BinaryPack.Models.Interfaces;
using MemoryPack;
using MessagePack;
using Orleans;
using ProtoBuf;

#nullable enable

namespace BinaryPack.Models
{
    /// <summary>
    /// A model that represents a container for a fake REST API response in JSON format
    /// </summary>
    [Serializable]
    [MemoryPackable]
    [MessagePackObject]
    [ProtoContract]
    [Orleans.GenerateSerializer]
    public sealed partial class JsonResponseModel : IInitializable, IEquatable<JsonResponseModel>
    {
        [MemoryPackConstructor]
        public JsonResponseModel() { }

        public JsonResponseModel(bool initialize)
        {
            if (initialize) Initialize();
        }

        [Key(0), Id(0), ProtoMember(1)]
        public string? Id { get; set; }

        [Key(1), Id(1), ProtoMember(2)]
        public string? Type { get; set; }

        [Key(2), Id(2), ProtoMember(3)]
        public int Count { get; set; }

        [Key(3), Id(3), ProtoMember(4)]
        public DateTime CreationTime { get; set; }

        [Key(4), Id(4), ProtoMember(5)]
        public DateTime UpdateTime { get; set; }

        [Key(5), Id(5), ProtoMember(6)]
        public DateTime ExpirationTime { get; set; }

        [Key(6), Id(6), ProtoMember(7)]
        public string? PreviousPageId { get; set; }

        [Key(7), Id(7), ProtoMember(8)]
        public string? FollowingPageId { get; set; }

        [Key(8), Id(8), ProtoMember(9)]
        public List<ApiModelContainer>? ModelContainers { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Id = RandomProvider.NextString(6);
            Type = nameof(JsonResponseModel);
            Count = RandomProvider.NextInt();
            CreationTime = RandomProvider.NextDateTime();
            UpdateTime = RandomProvider.NextDateTime();
            ExpirationTime = RandomProvider.NextDateTime();
            PreviousPageId = RandomProvider.NextString(6);
            FollowingPageId = RandomProvider.NextString(6);
            ModelContainers = new List<ApiModelContainer>();
            for (int i = 0; i < 50; i++)
            {
                var model = new ApiModelContainer();
                model.Initialize();
                ModelContainers.Add(model);
            }
        }

        /// <inheritdoc/>
        public bool Equals(JsonResponseModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Id == other.Id &&
                Type == other.Type &&
                Count == other.Count &&
                CreationTime.Equals(other.CreationTime) &&
                UpdateTime.Equals(other.UpdateTime) &&
                ExpirationTime.Equals(other.ExpirationTime) &&
                PreviousPageId == other.PreviousPageId &&
                FollowingPageId == other.FollowingPageId &&
                ModelContainers?.Count == other.ModelContainers?.Count &&
                ModelContainers.Zip(other.ModelContainers).All(p => p.First.Equals(p.Second));
        }
    }

    /// <summary>
    /// A model that represents a container for a fake API response
    /// </summary>
    [Serializable]
    [MemoryPackable]
    [MessagePackObject]
    [ProtoContract]
    [Orleans.GenerateSerializer]
    public sealed partial class ApiModelContainer : IInitializable, IEquatable<ApiModelContainer>
    {
        [Key(0), Id(0), ProtoMember(1)]
        public string? Id { get; set; }

        [Key(1), Id(1), ProtoMember(2)]
        public string? Type { get; set; }

        [Key(2), Id(2), ProtoMember(3)]
        public RestApiModel? Model { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Id = RandomProvider.NextString(6);
            Type = nameof(JsonResponseModel);
            Model = new RestApiModel();
            Model.Initialize();
        }

        /// <inheritdoc/>
        public bool Equals(ApiModelContainer? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) throw new InvalidOperationException();
            return
                Id?.Equals(other.Id) == true &&
                Type?.Equals(other.Type) == true &&
                Model?.Equals(other.Model) == true;
        }
    }

    /// <summary>
    /// A model that represents a REST API response for a single item
    /// </summary>
    [Serializable]
    [MemoryPackable]
    [MessagePackObject]
    [ProtoContract]
    [Orleans.GenerateSerializer]
    public sealed partial class RestApiModel : IInitializable, IEquatable<RestApiModel>
    {
        [Key(0), Id(0), ProtoMember(1)]
        public string? Id { get; set; }

        [Key(1), Id(1), ProtoMember(2)]
        public string? Type { get; set; }

        [Key(2), Id(2), ProtoMember(3)]
        public string? Parent { get; set; }

        [Key(3), Id(3), ProtoMember(4)]
        public string? Author { get; set; }

        [Key(4), Id(4), ProtoMember(5)]
        public string? Title { get; set; }

        [Key(5), Id(5), ProtoMember(6)]
        public string? Text { get; set; }

        [Key(6), Id(6), ProtoMember(7)]
        public string? Url { get; set; }

        [Key(7), Id(7), ProtoMember(8)]
        public string? HtmlContent { get; set; }

        [Key(8), Id(8), ProtoMember(9)]
        public int Upvotes { get; set; }

        [Key(9), Id(9), ProtoMember(10)]
        public int Downvotes { get; set; }

        [Key(10), Id(10), ProtoMember(11)]
        public float VotesRatio { get; set; }

        [Key(11), Id(11), ProtoMember(12)]
        public int Views { get; set; }

        [Key(12), Id(12), ProtoMember(13)]
        public int Clicks { get; set; }

        [Key(13), Id(13), ProtoMember(14)]
        public float ClicksRatio { get; set; }

        [Key(14), Id(14), ProtoMember(15)]
        public int NumberOfComments { get; set; }

        [Key(15), Id(15), ProtoMember(16)]
        public DateTime CreationTime { get; set; }

        [Key(16), Id(16), ProtoMember(17)]
        public DateTime UpdateTime { get; set; }

        [Key(17), Id(17), ProtoMember(18)]
        public DateTime ExpirationTime { get; set; }

        [Key(18), Id(18), ProtoMember(19)]
        public bool Flag1 { get; set; }

        [Key(19), Id(19), ProtoMember(20)]
        public bool Flag2 { get; set; }

        [Key(20), Id(20), ProtoMember(21)]
        public bool Flag3 { get; set; }

        [Key(21), Id(21), ProtoMember(22)]
        public bool Flag4 { get; set; }

        [Key(22), Id(22), ProtoMember(23)]
        public bool Flag5 { get; set; }

        [Key(23), Id(23), ProtoMember(24)]
        public string? Optional1 { get; set; }

        [Key(24), Id(24), ProtoMember(25)]
        public string? Optional2 { get; set; }

        [Key(25), Id(25), ProtoMember(26)]
        public string? Optional3 { get; set; }

        [Key(26), Id(26), ProtoMember(27)]
        public MediaInfoModel? Info { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Id = RandomProvider.NextString(6);
            Type = nameof(RestApiModel);
            Parent = RandomProvider.NextString(6);
            Author = RandomProvider.NextString(6);
            Title = RandomProvider.NextString(RandomProvider.NextInt(40, 120));
            if (RandomProvider.NextBool())
            {

                Text = RandomProvider.NextString(RandomProvider.NextInt(80, 400));
                Url = null;
                HtmlContent = RandomProvider.NextString(RandomProvider.NextInt(100, 600));
            }
            else
            {
                Text = null;
                Url = RandomProvider.NextString(RandomProvider.NextInt(80, 120));
                HtmlContent = null;
            }
            Upvotes = RandomProvider.NextInt();
            Downvotes = RandomProvider.NextInt();
            VotesRatio = Upvotes / (float)Downvotes;
            Views = RandomProvider.NextInt();
            Clicks = RandomProvider.NextInt();
            ClicksRatio = Views / (float)Clicks;
            NumberOfComments = RandomProvider.NextInt();
            CreationTime = RandomProvider.NextDateTime();
            UpdateTime = RandomProvider.NextDateTime();
            ExpirationTime = RandomProvider.NextDateTime();
            Flag1 = RandomProvider.NextBool();
            Flag2 = RandomProvider.NextBool();
            Flag3 = RandomProvider.NextBool();
            Flag4 = RandomProvider.NextBool();
            Flag5 = RandomProvider.NextBool();
            if (RandomProvider.NextBool()) Optional1 = RandomProvider.NextString(RandomProvider.NextInt(6, 20));
            if (RandomProvider.NextBool()) Optional2 = RandomProvider.NextString(RandomProvider.NextInt(6, 20));
            if (RandomProvider.NextBool()) Optional3 = RandomProvider.NextString(RandomProvider.NextInt(6, 20));
            if (RandomProvider.NextBool())
            {
                Info = new MediaInfoModel();
                Info.Initialize();
            }
        }

        /// <inheritdoc/>
        public bool Equals(RestApiModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Id == other.Id &&
                Type == other.Type &&
                Parent == other.Parent &&
                Author == other.Author &&
                Title == other.Title &&
                Text == other.Text &&
                Url == other.Url &&
                HtmlContent == other.HtmlContent &&
                Upvotes == other.Upvotes &&
                Downvotes == other.Downvotes &&
                MathF.Abs(VotesRatio - other.VotesRatio) < 0.001f &&
                Views == other.Views &&
                Clicks == other.Clicks &&
                MathF.Abs(ClicksRatio - other.ClicksRatio) < 0.001f &&
                NumberOfComments == other.NumberOfComments &&
                CreationTime.Equals(other.CreationTime) &&
                UpdateTime.Equals(other.UpdateTime) &&
                ExpirationTime.Equals(other.ExpirationTime) &&
                Flag1 == other.Flag1 &&
                Flag2 == other.Flag2 &&
                Flag3 == other.Flag3 &&
                Flag4 == other.Flag4 &&
                Flag5 == other.Flag5 &&
                Optional1 == other.Optional1 &&
                Optional2 == other.Optional2 &&
                Optional3 == other.Optional3 &&
                (Info == null && other.Info == null ||
                 Info?.Equals(other.Info) == true);
        }
    }

    /// <summary>
    /// A model that represents a collection of fake images
    /// </summary>
    [Serializable]
    [MemoryPackable]
    [MessagePackObject]
    [ProtoContract]
    [Orleans.GenerateSerializer]
    public sealed partial class MediaInfoModel : IInitializable, IEquatable<MediaInfoModel>
    {
        [Key(0), Id(0), ProtoMember(1)]
        public string? Id { get; set; }

        [Key(1), Id(1), ProtoMember(2)]
        public string? AlbumUrl { get; set; }

        [Key(2), Id(2), ProtoMember(3)]
        public bool Property { get; set; }

        [Key(3), Id(3), ProtoMember(4)]
        public List<ImageModel>? Images { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Id = RandomProvider.NextString(6);
            AlbumUrl = RandomProvider.NextString(100);
            Property = RandomProvider.NextBool();
            Images = new List<ImageModel>();
            int count = RandomProvider.NextInt() % 4 + 1;
            for (int i = 0; i < count; i++)
            {
                var model = new ImageModel();
                model.Initialize();
                Images.Add(model);
            }
        }

        /// <inheritdoc/>
        public bool Equals(MediaInfoModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Id?.Equals(other.Id) == true &&
                AlbumUrl?.Equals(other.AlbumUrl) == true &&
                Property == other.Property &&
                Images?.Count == other.Images?.Count &&
                Images.Zip(other.Images).All(p => p.First.Equals(p.Second));
        }
    }

    /// <summary>
    /// A simple model that contains a fake URL to an image and some metadata
    /// </summary>
    [Serializable]
    [MemoryPackable]
    [MessagePackObject]
    [ProtoContract]
    [Orleans.GenerateSerializer]
    public sealed partial class ImageModel : IInitializable, IEquatable<ImageModel>
    {
        [Key(0), Id(0), ProtoMember(1)]
        public string? Url { get; set; }

        [Key(1), Id(1), ProtoMember(2)]
        public int Width { get; set; }

        [Key(2), Id(2), ProtoMember(3)]
        public int Height { get; set; }

        [Key(3), Id(3), ProtoMember(4)]
        public float AspectRatio { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Url = RandomProvider.NextString(RandomProvider.NextInt(140, 200));
            Width = RandomProvider.NextInt();
            Height = RandomProvider.NextInt();
            AspectRatio = Width / (float)Height;
        }

        /// <inheritdoc/>
        public bool Equals(ImageModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) throw new InvalidOperationException();
            return
                Url?.Equals(other.Url) == true &&
                Width == other.Width &&
                Height == other.Height &&
                MathF.Abs(AspectRatio - other.AspectRatio) < 0.001f;
        }
    }
}
