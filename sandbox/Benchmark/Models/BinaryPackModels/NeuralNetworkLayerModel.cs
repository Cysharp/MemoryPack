using System;
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
    /// A model that represents an example of a neural network model
    /// </summary>
    [Serializable]
    [MemoryPackable]
    [MessagePackObject]
    [ProtoContract]
    public sealed partial class NeuralNetworkLayerModel : IInitializable, IEquatable<NeuralNetworkLayerModel>
    {
        [Key(0), Id(0), ProtoMember(1)]
        public string? Id { get; set; }

        [Key(1), Id(1), ProtoMember(2)]
        public int Index { get; set; }

        [Key(2), Id(2), ProtoMember(3)]
        public int Inputs { get; set; }

        [Key(3), Id(3), ProtoMember(4)]
        public int Outputs { get; set; }

        [Key(4), Id(4), ProtoMember(5)]
        public float[]? Weights { get; set; }

        [Key(5), Id(5), ProtoMember(6)]
        public float[]? Biases { get; set; }

        [Key(6), Id(6), ProtoMember(7)]
        public ActivationType Activation { get; set; }

        [Key(7), Id(7), ProtoMember(8)]
        public DateTime LastUpdateTime { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Id = RandomProvider.NextString(16);
            Index = RandomProvider.NextInt();
            Inputs = RandomProvider.NextInt();
            Outputs = RandomProvider.NextInt();
            Weights = new float[65536];
            for (int i = 0; i < Weights.Length; i++)
            {
                Weights[i] = (float)RandomProvider.NextDouble();
            }
            Biases = new float[256];
            for (int i = 0; i < Biases.Length; i++)
            {
                Biases[i] = (float)RandomProvider.NextDouble();
            }
            Activation = ActivationType.Sigmoid;
            LastUpdateTime = DateTime.Now;
        }

        /// <inheritdoc/>
        public bool Equals(NeuralNetworkLayerModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                (Id == null && other.Id == null ||
                 Id?.Equals(other.Id) == true) &&
                Index == other.Index &&
                Inputs == other.Inputs &&
                Outputs == other.Outputs &&
                (Weights == null && other.Weights == null ||
                 Weights?.Length == other.Weights?.Length &&
                 Weights?.Zip(other.Weights).All(t => MathF.Abs(t.First - t.Second) < 0.001f) == true) &&
                (Biases == null && other.Biases == null ||
                 Biases?.Length == other.Biases?.Length &&
                 Biases?.Zip(other.Biases).All(t => MathF.Abs(t.First - t.Second) < 0.001f) == true) &&
                Activation == other.Activation &&
                LastUpdateTime.Equals(other.LastUpdateTime);
        }
    }

    /// <summary>
    /// A sample <see langword="enum"/> representing an activation function type for a neural network layer
    /// </summary>
    public enum ActivationType
    {
        Sigmoid
    }
}
