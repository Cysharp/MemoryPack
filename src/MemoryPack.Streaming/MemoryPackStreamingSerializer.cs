using MemoryPack.Internal;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace MemoryPack.Streaming;

public static class MemoryPackStreamingSerializer
{
    public static async ValueTask SerializeAsync<T>(PipeWriter pipeWriter, int count, IEnumerable<T> source, int flushRate = 4096, CancellationToken cancellationToken = default)
    {
        static void WriteCollectionHeader(PipeWriter pipeWriter, int count)
        {
            var writer = new MemoryPackWriter<PipeWriter>(ref pipeWriter);
            writer.WriteCollectionHeader(count);
            writer.Flush();
        }

        static bool WriteWhileReachFlushRate(PipeWriter pipeWriter, IEnumerator<T> enumerator, int flushRate)
        {
            var writer = new MemoryPackWriter<PipeWriter>(ref pipeWriter);
            while (enumerator.MoveNext())
            {
                writer.WriteObject(enumerator.Current);
                if (flushRate < writer.WrittenCount)
                {
                    writer.Flush();
                    return true;
                }
            }

            writer.Flush();
            return false; // false when completed.
        }

        WriteCollectionHeader(pipeWriter, count);

        using var enumerator = source.GetEnumerator();

        while (WriteWhileReachFlushRate(pipeWriter, enumerator, flushRate))
        {
            await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask SerializeAsync<T>(Stream stream, int count, IEnumerable<T> source, int flushRate = 4096, CancellationToken cancellationToken = default)
    {
        static void WriteCollectionHeader(ReusableLinkedArrayBufferWriter bufferWriter, int count)
        {
            var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufferWriter);
            writer.WriteCollectionHeader(count);
            writer.Flush();
        }

        static bool WriteWhileReachFlushRate(ReusableLinkedArrayBufferWriter bufferWriter, IEnumerator<T> enumerator, int flushRate)
        {
            var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufferWriter);
            while (enumerator.MoveNext())
            {
                writer.WriteObject(enumerator.Current);
                if (flushRate < writer.WrittenCount)
                {
                    writer.Flush();
                    return true;
                }
            }

            writer.Flush();
            return false; // false when completed.
        }

        var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            WriteCollectionHeader(tempWriter, count);

            using var enumerator = source.GetEnumerator();

            while (WriteWhileReachFlushRate(tempWriter, enumerator, flushRate))
            {
                await tempWriter.WriteToAndResetAsync(stream, cancellationToken).ConfigureAwait(false);
            }
            await tempWriter.WriteToAndResetAsync(stream, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
        }
    }

    public static async IAsyncEnumerable<T?> DeserializeAsync<T>(PipeReader pipeReader, int bufferAtLeast = 4096, int readMinimumSize = 8192, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        static bool ReadCollectionHeader(in ReadOnlySequence<byte> buffer, out int length)
        {
            using var reader = new MemoryPackReader(buffer);

            // allow to use `Dangerous` read header.
            return reader.DangerousTryReadCollectionHeader(out length);
        }

        static int Deserialize(in ReadOnlySequence<byte> buffer, int bufferAtLeast, List<T?> itemBuffer, StrongBox<int> remain, bool bufferIsFull)
        {
            using var reader = new MemoryPackReader(buffer);

            while (bufferIsFull || bufferAtLeast < reader.Remaining)
            {
                if (remain.Value == 0)
                {
                    return reader.Consumed;
                }

                itemBuffer.Add(reader.ReadObject<T?>());
                remain.Value--;
            }

            return reader.Consumed;
        }

        if (readMinimumSize < bufferAtLeast)
        {
            throw new ArgumentException($"readMinimumSize must larger than bufferAtLeast. readMinimumSize: {readMinimumSize} bufferAtLeast:{bufferAtLeast}");
        }

        var itemBuffer = new List<T?>();
        var readResult = await pipeReader.ReadAtLeastAsync(readMinimumSize, cancellationToken).ConfigureAwait(false);

        if (!readResult.IsCanceled)
        {
            var buffer = readResult.Buffer;
            if (ReadCollectionHeader(buffer, out var length))
            {
                pipeReader.AdvanceTo(buffer.GetPosition(4));
                if (readResult.IsCompleted)
                {
                    buffer = buffer.Slice(4);
                }

                var remain = new StrongBox<int>(length);
                while (remain.Value != 0)
                {
                    if (!readResult.IsCompleted)
                    {
                        readResult = await pipeReader.ReadAtLeastAsync(readMinimumSize, cancellationToken).ConfigureAwait(false);
                        buffer = readResult.Buffer;
                    }

                    if (readResult.IsCanceled)
                    {
                        yield break;
                    }

                    var consumedByteCount = Deserialize(buffer, bufferAtLeast, itemBuffer, remain, readResult.IsCompleted);

                    if (itemBuffer.Count > 0)
                    {
                        foreach (var item in itemBuffer)
                        {
                            yield return item;
                        }
                        itemBuffer.Clear();
                    }

                    if (readResult.IsCompleted)
                    {
                        buffer = buffer.Slice(consumedByteCount);

                        if (consumedByteCount == 0 || buffer.Length == 0)
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        pipeReader.AdvanceTo(buffer.GetPosition(consumedByteCount));
                    }
                }
            }
        }

        foreach (var item in itemBuffer)
        {
            yield return item;
        }
    }

    public static IAsyncEnumerable<T?> DeserializeAsync<T>(Stream stream, int bufferAtLeast = 4096, int readMinimumSize = 8192, CancellationToken cancellationToken = default)
    {
        return DeserializeAsync<T>(PipeReader.Create(stream), bufferAtLeast, readMinimumSize, cancellationToken);
    }
}
