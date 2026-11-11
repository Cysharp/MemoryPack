using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

/// <summary>
/// <see cref="Nullable{T}"/> compatible struct that has no padding.
/// <para>
/// <see cref="Nullable{T}"/> of unmanaged type is serialized as its memory layout(bool hasValue, padding, T value) and the padding is not initialized,
/// so the binary of the same value is not always same and it may include the garbage of memory.
/// This type is (T hasValue, T value) so there is no padding, the binary of the same value is always same.
/// </para>
/// <para>
/// When T is primitive type(int, long, double, bool, etc...) or enum, the binary is compatible with <see cref="Nullable{T}"/>(the both can read each other).
/// In other types(Guid, decimal, user-defined struct, etc...), the binary is NOT compatible with <see cref="Nullable{T}"/>.
/// </para>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SafeNullable<T> : IEquatable<SafeNullable<T>>
    where T : unmanaged
{
    // Nullable<T> is (bool hasValue, [alignment of T - 1] bytes padding, T value).
    // If size of T is same as alignment of T(primitives, enum), (T hasValue, T value) is the same layout and has no padding.
    // Write: first byte of hasValue is 0 or 1, others are zero.
    // Read: only uses first byte of hasValue, it can read the binary of Nullable<T> that padding is not initialized.

    readonly T hasValue;
    readonly T value;

    public SafeNullable(T value)
    {
        this.hasValue = CreateHasValue();
        this.value = value;
    }

    public SafeNullable(T? value)
    {
        if (value.HasValue)
        {
            this.hasValue = CreateHasValue();
            this.value = value.GetValueOrDefault();
        }
        else
        {
            this.hasValue = default;
            this.value = default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static T CreateHasValue()
    {
        var hasValue = default(T);
        Unsafe.As<T, byte>(ref hasValue) = 1;
        return hasValue;
    }

    public bool HasValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return Unsafe.As<T, byte>(ref Unsafe.AsRef(in hasValue)) != 0;
        }
    }

    public T Value
    {
        get
        {
            if (!HasValue)
            {
                throw new InvalidOperationException("SafeNullable object must have a value.");
            }
            return value;
        }
    }

    public T GetValueOrDefault()
    {
        return HasValue ? value : default;
    }

    public T GetValueOrDefault(T defaultValue)
    {
        return HasValue ? value : defaultValue;
    }

    public T? ToNullable()
    {
        return HasValue ? value : null;
    }

    public static implicit operator SafeNullable<T>(T? value)
    {
        return new SafeNullable<T>(value);
    }

    public static implicit operator T?(SafeNullable<T> value)
    {
        return value.ToNullable();
    }

    public bool Equals(SafeNullable<T> other)
    {
        if (!HasValue) return !other.HasValue;
        if (!other.HasValue) return false;
        return EqualityComparer<T>.Default.Equals(value, other.value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is SafeNullable<T> other) return Equals(other);
        if (obj == null) return !HasValue;
        if (obj is T otherValue) return HasValue && EqualityComparer<T>.Default.Equals(value, otherValue);
        return false;
    }

    public override int GetHashCode()
    {
        return HasValue ? value.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return HasValue ? (value.ToString() ?? "") : "";
    }
}
