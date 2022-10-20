using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests.Models;

// no error generatable.
#pragma warning disable CS8618
[MemoryPackable]
public partial class HogeHoge
{
    public BigInteger P1;
    public Version P2;
    public Uri P3;
    public TimeZoneInfo P4;
    public BitArray P5;
    public StringBuilder P6;
    public Type P7;
    public int[,] P8;
    public int[,,] P9;
    public int[,,,] P10;
    // generics
    public KeyValuePair<int, int> P11;
    public Lazy<int> P12;
    public Nullable<int> P13;
    // collecition
    public ArraySegment<int> P14;
    public Memory<int> P15;
    public ReadOnlyMemory<int> P16;
    public ReadOnlySequence<int> P17;

    public List<int> P18;
    public Stack<int> P19;
    public Queue<int> P20;
    public LinkedList<int> P21;
    public HashSet<int> P22;
#if NET7_0_OR_GREATER
    public PriorityQueue<int, int> P23;
#endif
    public ObservableCollection<int> P24;
    public Collection<int> P25;
    public ConcurrentQueue<int> P26;
    public ConcurrentStack<int> P27;
    public ConcurrentBag<int> P28;
    public Dictionary<int, int> P29;
    public SortedDictionary<int, int> P30;
    public SortedList<int, int> P31;
    public ConcurrentDictionary<int, int> P32;
    public ReadOnlyCollection<int> P33;
    public ReadOnlyObservableCollection<int> P34;
    public BlockingCollection<int> P35;

    public ImmutableArray<int> P36;
    public ImmutableList<int> P37;
    public ImmutableQueue<int> P38;
    public ImmutableStack<int> P39;
    public ImmutableDictionary<int, int> P40;
    public ImmutableSortedDictionary<int, int> P41;
    public ImmutableSortedSet<int> P42;
    public ImmutableHashSet<int> P43;
    public IImmutableList<int> P44;
    public IImmutableQueue<int> P45;
    public IImmutableStack<int> P46;
    public IImmutableDictionary<int, int> P47;
    public IImmutableSet<int> P48;
    public IEnumerable<int> P49;
    public ICollection<int> P50;
    public IReadOnlyCollection<int> P51;
    public IList<int> P52;
    public IReadOnlyList<int> P53;
    public IDictionary<int, int> P54;
    public IReadOnlyDictionary<int, int> P55;
    public ILookup<int, int> P56;
    public IGrouping<int, int> P57;
    public ISet<int> P58;
#if NET7_0_OR_GREATER
    public IReadOnlySet<int> P59;
#endif

    // tuples
    public Tuple<int, string, int> T3;
    public ValueTuple<int, string, int> VT3;
    // more
    public Nullable<MyStruct> N1;
    public KeyValuePair<string, string> N2;

    public IUnionType? U1;
}


[MemoryPackable]
public partial struct MyStruct
{
    public string? V;
}

[MemoryPackUnion(0, typeof(AUnion))]
public partial interface IUnionType
{

}



[MemoryPackable]
public partial class AUnion : IUnionType
{

}
