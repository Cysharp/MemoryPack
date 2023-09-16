﻿using MemoryPack.Internal;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters;

[Preserve]
public sealed class BigIntegerFormatter : MemoryPackFormatter<BigInteger>
{
    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref BigInteger value)
    {
#if !UNITY_2021_2_OR_NEWER
        Span<byte> temp = stackalloc byte[255];
        if (value.TryWriteBytes(temp, out var written))
        {
            writer.WriteUnmanagedSpan(temp.Slice(written));
            return;
        }
        else
#endif
        {
            var byteArray = value.ToByteArray();
            writer.WriteUnmanagedArray(byteArray);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref BigInteger value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        ref var src = ref reader.GetSpanReference(length);
        value = new BigInteger(MemoryMarshal.CreateReadOnlySpan(ref src, length));

        reader.Advance(length);
    }
}
