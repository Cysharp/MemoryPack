using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

#nullable enable

namespace BinaryPack.Models.Helpers
{
    /// <summary>
    /// A helper <see langword="class"/> with methods to match instances of specific types
    /// </summary>
    internal static class StructuralComparer
    {
        /// <summary>
        /// Checks whether or not two input <typeparamref name="T"/> values match
        /// </summary>
        /// <typeparam name="T">The type of items to compare</typeparam>
        /// <param name="a">The first <typeparamref name="T"/> value to compare</param>
        /// <param name="b">The second <typeparamref name="T"/> value to compare</param>
        /// <returns><see langword="true"/> if both instances are either <see langword="null"/> or matching, <see langword="false"/> otherwise</returns>
        [Pure]
        public static bool IsMatch<T>(T? a, T? b) where T : class, IEquatable<T>
        {
            if (a != null && b != null) return a.Equals(b);
            return a == null && b == null;
        }

        /// <summary>
        /// Checks whether or not two input <see cref="Nullable{T}"/> values match
        /// </summary>
        /// <typeparam name="T">The type of items to compare</typeparam>
        /// <param name="a">The first <see cref="Nullable{T}"/> value to compare</param>
        /// <param name="b">The second <see cref="Nullable{T}"/> value to compare</param>
        /// <returns><see langword="true"/> if both instances are either <see langword="null"/> or matching, <see langword="false"/> otherwise</returns>
        [Pure]
        public static bool IsMatch<T>(T? a, T? b) where T : struct, IEquatable<T>
        {
            if (a != null && b != null) return a.Equals(b);
            return a == null && b == null;
        }

        /// <summary>
        /// Checks whether or not two input <see cref="IEnumerable{T}"/> instances represent a structural match
        /// </summary>
        /// <typeparam name="T">The type of items in the input <see cref="IEnumerable{T}"/> instances</typeparam>
        /// <param name="a">The first <see cref="IEnumerable{T}"/> instance</param>
        /// <param name="b">The second <see cref="IEnumerable{T}"/> instance</param>
        /// <returns><see langword="true"/> if both instances are either <see langword="null"/> or matching, <see langword="false"/> otherwise</returns>
        [Pure]
        public static bool IsMatch<T>(IEnumerable<T>? a, IEnumerable<T>? b) where T : IEquatable<T>
        {
            if (a != null && b != null)
            {
                if (a.Count() != b.Count()) return false;
                foreach ((T first, T second) in a.Zip(b))
                {
                    if (!(first != null && second != null && first.Equals(second) ||
                          first == null && second == null)) return false;
                }

                return true;
            }

            return a == null && b == null;
        }

        /// <summary>
        /// Checks whether or not the two input <see cref="IDictionary{TKey,TValue}"/> instances represent a structural match
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the input <see cref="IDictionary{TKey,TValue}"/> instances</typeparam>
        /// <typeparam name="TValue">The type of values in the input <see cref="IDictionary{TKey,TValue}"/> instances</typeparam>
        /// <param name="a">The first <see cref="IDictionary{TKey,TValue}"/> instance</param>
        /// <param name="b">The second <see cref="IDictionary{TKey,TValue}"/> instance</param>
        /// <returns><see langword="true"/> if both instances are either <see langword="null"/> or matching, <see langword="false"/> otherwise</returns>
        [Pure]
        public static bool IsMatch<TKey, TValue>(IDictionary<TKey, TValue?>? a, IDictionary<TKey, TValue?>? b)
            where TKey : IEquatable<TKey>
            where TValue : class, IEquatable<TValue>
        {
            if (a != null && b != null)
            {
                if (a.Count != b.Count) return false;
                foreach ((TKey k, TValue? aValue) in a)
                {
                    if (!b.TryGetValue(k, out TValue? bValue)) return false;
                    if (!(aValue != null && bValue != null && aValue.Equals(bValue) ||
                          aValue == null && bValue == null)) return false;
                }

                return true;
            }

            return a == null && b == null;
        }

        /// <summary>
        /// Checks whether or not the two input <see cref="IDictionary{TKey,TValue}"/> instances represent a structural match
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the input <see cref="IDictionary{TKey,TValue}"/> instances</typeparam>
        /// <typeparam name="TValue">The type of <see cref="Nullable{T}"/> values in the input <see cref="IDictionary{TKey,TValue}"/> instances</typeparam>
        /// <param name="a">The first <see cref="IDictionary{TKey,TValue}"/> instance</param>
        /// <param name="b">The second <see cref="IDictionary{TKey,TValue}"/> instance</param>
        /// <returns><see langword="true"/> if both instances are either <see langword="null"/> or matching, <see langword="false"/> otherwise</returns>
        [Pure]
        public static bool IsMatch<TKey, TValue>(IDictionary<TKey, TValue?>? a, IDictionary<TKey, TValue?>? b)
            where TKey : IEquatable<TKey>
            where TValue : struct, IEquatable<TValue>
        {
            if (a != null && b != null)
            {
                if (a.Count != b.Count) return false;
                foreach ((TKey k, TValue? aValue) in a)
                {
                    if (!b.TryGetValue(k, out TValue? bValue)) return false;
                    if (!(aValue != null && bValue != null && aValue.Value.Equals(bValue.Value) ||
                          aValue == null && bValue == null)) return false;
                }

                return true;
            }

            return a == null && b == null;
        }
    }
}
