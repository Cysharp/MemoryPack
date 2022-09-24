using MemoryPack.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Formatters;

// interface and other(not clear support) formatters
// ReadOnlyCollection, ReadOnlyObservableCollection, BlockingCollection
// IEnumerable, ICollection, IReadOnlyCollection, IList, IReadOnlyList
// IDictionary, ILookup, IGrouping, ISet, IReadOnlySet, IReadOnlyDictionary

// TODO:not impl yet
class InterfaceCollectionFormatters
{



    public InterfaceCollectionFormatters()
    {
        new ConcurrentDictionary<int, int>().Clear();
    }


}


public sealed class EnumerableFormatter<T> : MemoryPackFormatter<IEnumerable<T>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref IEnumerable<T>? value)
    {
        if (value == null)
        {
            writer.WriteNullLengthHeader();
            return;
        }

        if (TryGetNonEnumeratedCount(value, out var count))
        {
            writer.WriteLengthHeader(count);
            foreach (var item in value)
            {
                // TODO: write item
            }
        }
        else
        {
            var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
            try
            {
                var tempContext = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref tempWriter);

                foreach (var item in value)
                {
                    // TODO: write item to tempContext
                }

                tempContext.Flush();

                writer.WriteLengthHeader(tempWriter.TotalWritten);
                tempWriter.WriteToAndReset(ref writer);
            }
            finally
            {
                tempWriter.Reset();
                ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
            }
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref IEnumerable<T>? value)
    {
        // TODO:...
        throw new NotImplementedException();
    }

    static bool TryGetNonEnumeratedCount(IEnumerable<T> value, out int count)
    {
        // TryGetNonEnumeratedCount is not check IReadOnlyCollection<T> so add check manually.
        // https://github.com/dotnet/runtime/issues/54764

        if (value.TryGetNonEnumeratedCount(out count))
        {
            return true;
        }

        if (value is IReadOnlyCollection<T> readOnlyCollection)
        {
            count = readOnlyCollection.Count;
            return true;
        }

        return false;
    }
}
