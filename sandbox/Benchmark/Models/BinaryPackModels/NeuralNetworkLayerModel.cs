using System;
using System.Linq;
using BinaryPack.Models.Helpers;
using BinaryPack.Models.Interfaces;

#nullable enable

namespace BinaryPack.Models
{
    /// <summary>
    /// A model that represents an example of a neural network model
    /// </summary>
    [Serializable]
    public sealed class NeuralNetworkLayerModel : IInitializable, IEquatable<NeuralNetworkLayerModel>
    {
        public string? Id { get; set; }

        public int Index { get; set; }

        public int Inputs { get; set; }

        public int Outputs { get; set; }

        public float[]? Weights { get; set; }

        public float[]? Biases { get; set; }

        public ActivationType Activation { get; set; }

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
