using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;
using System.Buffers;
using System.IO.Compression;

namespace MemoryPack.Compression {

[Preserve]
public sealed class BrotliFormatter : MemoryPackFormatter<byte[]>
{
    public static readonly BrotliFormatter Default = new BrotliFormatter();

    readonly System.IO.Compression.CompressionLevel compressionLevel;

    public BrotliFormatter()
        : this(System.IO.Compression.CompressionLevel.Fastest)
    {

    }

    public BrotliFormatter(System.IO.Compression.CompressionLevel compressionLevel)
    {
        this.compressionLevel = compressionLevel;
    }

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref byte[]? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        if (value.Length == 0)
        {
            writer.WriteCollectionHeader(0);
            return;
        }

        var quality = BrotliUtils.GetQualityFromCompressionLevel(compressionLevel);
        var window = BrotliUtils.WindowBits_Default;

        using var encoder = new BrotliEncoder(quality, window);

        var maxLength = BrotliEncoder.GetMaxCompressedLength(value.Length);
        var finalBuffer = ArrayPool<byte>.Shared.Rent(maxLength);
        try
        {
            var writtenCount = 0;
            var destination = finalBuffer.AsSpan(0, maxLength);

            var status = encoder.Compress(value.AsSpan(), destination, out var bytesConsumed, out var bytesWritten, isFinalBlock: false);
            if (status != OperationStatus.Done)
            {
                MemoryPackSerializationException.ThrowCompressionFailed(status);
            }

            if (bytesConsumed != value.Length)
            {
                MemoryPackSerializationException.ThrowCompressionFailed();
            }

            if (bytesWritten > 0)
            {
                destination = destination.Slice(bytesWritten);
                writtenCount += bytesWritten;
            }

            // call BrotliEncoderOperation.Finish
            var finalStatus = encoder.Compress(ReadOnlySpan<byte>.Empty, destination, out var consumed, out var written, isFinalBlock: true);
            writtenCount += written;

            // write to MemoryPackWriter
            writer.WriteUnmanagedSpan(finalBuffer.AsSpan(0, writtenCount));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(finalBuffer);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref byte[]? value)
    {
        reader.DangerousReadUnmanagedSpanView<byte>(out var isNull, out var compressedBuffer);

        if (isNull)
        {
            value = null;
            return;
        }

        if (compressedBuffer.Length == 0)
        {
            value = Array.Empty<byte>();
            return;
        }

        using var decompressor = new BrotliDecompressor();

        var decompressedBuffer = decompressor.Decompress(compressedBuffer);
        var length = decompressedBuffer.Length;

        if (value == null || value.Length != length)
        {
            value = new byte[length];
        }

        decompressedBuffer.CopyTo(value);
    }
}

}