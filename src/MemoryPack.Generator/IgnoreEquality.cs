using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryPack.Generator;

public readonly struct IgnoreEquality<T>(T value) : IEquatable<IgnoreEquality<T>>
{
    public readonly T Value => value;

    public static implicit operator IgnoreEquality<T>(T value)
    {
        return new IgnoreEquality<T>(value);
    }

    public static implicit operator T(IgnoreEquality<T> value)
    {
        return value.Value;
    }

    public bool Equals(IgnoreEquality<T> other)
    {
        // always true to ignore equality check.
        return true;
    }
}
