
namespace MemoryPack.Formatters;

internal static class TupleFormatterTypes
{
    public static readonly Dictionary<Type, Type> TupleFormatters = new Dictionary<Type, Type>(16)
    {
        { typeof(Tuple<>), typeof(TupleFormatter<>) },
        { typeof(ValueTuple<>), typeof(ValueTupleFormatter<>) },
        { typeof(Tuple<,>), typeof(TupleFormatter<,>) },
        { typeof(ValueTuple<,>), typeof(ValueTupleFormatter<,>) },
        { typeof(Tuple<,,>), typeof(TupleFormatter<,,>) },
        { typeof(ValueTuple<,,>), typeof(ValueTupleFormatter<,,>) },
        { typeof(Tuple<,,,>), typeof(TupleFormatter<,,,>) },
        { typeof(ValueTuple<,,,>), typeof(ValueTupleFormatter<,,,>) },
        { typeof(Tuple<,,,,>), typeof(TupleFormatter<,,,,>) },
        { typeof(ValueTuple<,,,,>), typeof(ValueTupleFormatter<,,,,>) },
        { typeof(Tuple<,,,,,>), typeof(TupleFormatter<,,,,,>) },
        { typeof(ValueTuple<,,,,,>), typeof(ValueTupleFormatter<,,,,,>) },
        { typeof(Tuple<,,,,,,>), typeof(TupleFormatter<,,,,,,>) },
        { typeof(ValueTuple<,,,,,,>), typeof(ValueTupleFormatter<,,,,,,>) },
        { typeof(Tuple<,,,,,,,>), typeof(TupleFormatter<,,,,,,,>) },
        { typeof(ValueTuple<,,,,,,,>), typeof(ValueTupleFormatter<,,,,,,,>) },
    };
}

public sealed class TupleFormatter<T1> : MemoryPackFormatter<Tuple<T1?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Tuple<T1?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteObject(value.Item1);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Tuple<T1?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 1) MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);

        value = new Tuple<T1?>(
            reader.ReadObject<T1>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Tuple<T1?>? value)
    {
        throw new NotImplementedException();
    }
}

public sealed class TupleFormatter<T1, T2> : MemoryPackFormatter<Tuple<T1?, T2?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Tuple<T1?, T2?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(2);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Tuple<T1?, T2?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 2) MemoryPackSerializationException.ThrowInvalidPropertyCount(2, count);

        value = new Tuple<T1?, T2?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Tuple<T1?, T2?>? value)
    {
        throw new NotImplementedException();
    }
}

public sealed class TupleFormatter<T1, T2, T3> : MemoryPackFormatter<Tuple<T1?, T2?, T3?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Tuple<T1?, T2?, T3?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(3);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Tuple<T1?, T2?, T3?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 3) MemoryPackSerializationException.ThrowInvalidPropertyCount(3, count);

        value = new Tuple<T1?, T2?, T3?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Tuple<T1?, T2?, T3?>? value)
    {
        throw new NotImplementedException();
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Tuple<T1?, T2?, T3?, T4?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(4);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Tuple<T1?, T2?, T3?, T4?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 4) MemoryPackSerializationException.ThrowInvalidPropertyCount(4, count);

        value = new Tuple<T1?, T2?, T3?, T4?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Tuple<T1?, T2?, T3?, T4?>? value)
    {
        throw new NotImplementedException();
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4, T5> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?, T5?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(5);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
        writer.WriteObject(value.Item5);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 5) MemoryPackSerializationException.ThrowInvalidPropertyCount(5, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>(),
            reader.ReadObject<T5>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?>? value)
    {
        throw new NotImplementedException();
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(6);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
        writer.WriteObject(value.Item5);
        writer.WriteObject(value.Item6);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 6) MemoryPackSerializationException.ThrowInvalidPropertyCount(6, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>(),
            reader.ReadObject<T5>(),
            reader.ReadObject<T6>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?>? value)
    {
        throw new NotImplementedException();
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6, T7> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(7);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
        writer.WriteObject(value.Item5);
        writer.WriteObject(value.Item6);
        writer.WriteObject(value.Item7);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 7) MemoryPackSerializationException.ThrowInvalidPropertyCount(7, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>(),
            reader.ReadObject<T5>(),
            reader.ReadObject<T6>(),
            reader.ReadObject<T7>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>? value)
    {
        throw new NotImplementedException();
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
    where TRest : notnull
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(8);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
        writer.WriteObject(value.Item5);
        writer.WriteObject(value.Item6);
        writer.WriteObject(value.Item7);
        writer.WriteObject(value.Rest);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 8) MemoryPackSerializationException.ThrowInvalidPropertyCount(8, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>(),
            reader.ReadObject<T5>(),
            reader.ReadObject<T6>(),
            reader.ReadObject<T7>(),
            reader.ReadObject<TRest>()!
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>? value)
    {
        throw new NotImplementedException();
    }
}


public sealed class ValueTupleFormatter<T1> : MemoryPackFormatter<ValueTuple<T1?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ValueTuple<T1?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteObject(value.Item1);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ValueTuple<T1?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 1) MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);

        value = new ValueTuple<T1?>(
            reader.ReadObject<T1>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref ValueTuple<T1?> value)
    {
        throw new NotImplementedException();
    }
}

public sealed class ValueTupleFormatter<T1, T2> : MemoryPackFormatter<ValueTuple<T1?, T2?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ValueTuple<T1?, T2?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(2);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ValueTuple<T1?, T2?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 2) MemoryPackSerializationException.ThrowInvalidPropertyCount(2, count);

        value = new ValueTuple<T1?, T2?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref (T1?, T2?) value)
    {
        throw new NotImplementedException();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ValueTuple<T1?, T2?, T3?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(3);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ValueTuple<T1?, T2?, T3?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 3) MemoryPackSerializationException.ThrowInvalidPropertyCount(3, count);

        value = new ValueTuple<T1?, T2?, T3?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref (T1?, T2?, T3?) value)
    {
        throw new NotImplementedException();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ValueTuple<T1?, T2?, T3?, T4?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(4);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 4) MemoryPackSerializationException.ThrowInvalidPropertyCount(4, count);

        value = new ValueTuple<T1?, T2?, T3?, T4?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref (T1?, T2?, T3?, T4?) value)
    {
        throw new NotImplementedException();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(5);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
        writer.WriteObject(value.Item5);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 5) MemoryPackSerializationException.ThrowInvalidPropertyCount(5, count);

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>(),
            reader.ReadObject<T5>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref (T1?, T2?, T3?, T4?, T5?) value)
    {
        throw new NotImplementedException();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(6);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
        writer.WriteObject(value.Item5);
        writer.WriteObject(value.Item6);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 6) MemoryPackSerializationException.ThrowInvalidPropertyCount(6, count);

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>(),
            reader.ReadObject<T5>(),
            reader.ReadObject<T6>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref (T1?, T2?, T3?, T4?, T5?, T6?) value)
    {
        throw new NotImplementedException();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(7);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
        writer.WriteObject(value.Item5);
        writer.WriteObject(value.Item6);
        writer.WriteObject(value.Item7);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 7) MemoryPackSerializationException.ThrowInvalidPropertyCount(7, count);

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>(),
            reader.ReadObject<T5>(),
            reader.ReadObject<T6>(),
            reader.ReadObject<T7>()
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref (T1?, T2?, T3?, T4?, T5?, T6?, T7?) value)
    {
        throw new NotImplementedException();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
    where TRest : struct
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteObjectHeader(8);
        writer.WriteObject(value.Item1);
        writer.WriteObject(value.Item2);
        writer.WriteObject(value.Item3);
        writer.WriteObject(value.Item4);
        writer.WriteObject(value.Item5);
        writer.WriteObject(value.Item6);
        writer.WriteObject(value.Item7);
        writer.WriteObject(value.Rest);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default;
            return;
        }

        if (count != 8) MemoryPackSerializationException.ThrowInvalidPropertyCount(8, count);

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
            reader.ReadObject<T1>(),
            reader.ReadObject<T2>(),
            reader.ReadObject<T3>(),
            reader.ReadObject<T4>(),
            reader.ReadObject<T5>(),
            reader.ReadObject<T6>(),
            reader.ReadObject<T7>(),
            reader.ReadObject<TRest>()!
        );
    }

    public override void Serialize(ref DoNothingMemoryPackWriter writer, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value)
    {
        throw new NotImplementedException();
    }
}

