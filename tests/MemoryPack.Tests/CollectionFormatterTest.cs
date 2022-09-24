using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace MemoryPack.Tests;

public class CollectionFormatterTest
{
    void CollectionEqual<T>(T value)
        where T : IEnumerable<int>
    {
        var bin = MemoryPackSerializer.Serialize(value);
        var value2 = MemoryPackSerializer.Deserialize<T>(bin);
        value2.Should().Equal(value);
    }

    void CollectionEqualReference<T>(ref T? value, Action<T?> clear)
        where T : class, IEnumerable<int>
    {
        var bin = MemoryPackSerializer.Serialize(value);
        var original = value;
        clear(value);
        var expected = MemoryPackSerializer.Deserialize<T>(bin);
        MemoryPackSerializer.Deserialize<T>(bin, ref value);
        value.Should().Equal(expected);
        value.Should().BeSameAs(original);
    }

    [Fact]
    public void List()
    {
        var list = new List<int>() { 1, 2, 3, 4, 5 };
        var bin = MemoryPackSerializer.Serialize(list);

        // no ref
        MemoryPackSerializer.Deserialize<List<int>>(bin).Should().Equal(list);

        // ref and same length
        var list2 = new List<int>() { 10, 20, 30, 40, 50 };
        MemoryPackSerializer.Deserialize(bin, ref list2);
        list2.Should().Equal(list);

        // ref and differenct length
        var list3 = new List<int>() { 99, 98, 97 };
        MemoryPackSerializer.Deserialize(bin, ref list3);
        list3.Should().Equal(list);
    }

    [Fact]
    public void Stack()
    {
        void Push(Stack<int> stack, params int[] values)
        {
            foreach (var item in values)
            {
                stack.Push(item);
            }
        }

        var stack = new Stack<int>();
        Push(stack, 1, 2, 3, 4, 5);
        var bin = MemoryPackSerializer.Serialize(stack);

        // no ref
        MemoryPackSerializer.Deserialize<Stack<int>>(bin).Should().Equal(stack);

        // ref and same length
        var stack2 = new Stack<int>();
        Push(stack2, 10, 20, 30, 40, 50);
        MemoryPackSerializer.Deserialize(bin, ref stack2);
        stack2.Should().Equal(stack);

        // ref and differenct length
        var stack3 = new Stack<int>();
        Push(stack3, 99, 98, 97);
        MemoryPackSerializer.Deserialize(bin, ref stack3);
        stack3.Should().Equal(stack);
    }

    [Fact]
    public void Queue()
    {
        var q = new Queue<int>();
        q.Enqueue(1);
        q.Enqueue(2);
        q.Enqueue(3);
        q.Enqueue(4);
        q.Enqueue(5);

        CollectionEqual(q);
        CollectionEqualReference(ref q, x => x!.Clear());
    }

    [Fact]
    public void LinkedList()
    {
        var list = new LinkedList<int>();
        list.AddLast(1);
        list.AddLast(2);
        list.AddLast(3);
        list.AddLast(4);
        list.AddLast(5);

        CollectionEqual(list);
        CollectionEqualReference(ref list, x => list!.Clear());
    }

    [Fact]
    public void HashSet()
    {
        var collection = new HashSet<int>();
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);
        collection.Add(4);
        collection.Add(5);

        CollectionEqual(collection);
        CollectionEqualReference(ref collection, x => collection!.Clear());
    }

    [Fact]
    public void PriorityQueue()
    {
        var collection = new PriorityQueue<int, int>();
        collection.Enqueue(1, 10);
        collection.Enqueue(2, 4);
        collection.Enqueue(3, 1231);
        collection.Enqueue(4, 5);
        collection.Enqueue(5, 7);

        var bin = MemoryPackSerializer.Serialize(collection);
        var v2 = MemoryPackSerializer.Deserialize<PriorityQueue<int, int>>(bin);

        Debug.Assert(v2 != null);
        collection.Dequeue().Should().Be(v2.Dequeue());
        collection.Dequeue().Should().Be(v2.Dequeue());
        collection.Dequeue().Should().Be(v2.Dequeue());
        collection.Dequeue().Should().Be(v2.Dequeue());
        collection.Dequeue().Should().Be(v2.Dequeue());
    }

    [Fact]
    public void Collection()
    {
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            CollectionEqual(collection);
            CollectionEqualReference(ref collection, x => collection!.Clear());
        }
        {
            var collection = new Collection<int> { 1, 2, 3, 4, 5 };
            CollectionEqual(collection);
            CollectionEqualReference(ref collection, x => collection!.Clear());
        }
        {
            var collection = new ConcurrentQueue<int>();
            collection.Enqueue(1);
            collection.Enqueue(2);
            collection.Enqueue(3);
            collection.Enqueue(4);
            collection.Enqueue(5);
            CollectionEqual(collection);
            CollectionEqualReference(ref collection, x => collection!.Clear());
        }
        {
            var collection = new ConcurrentStack<int>();
            collection.Push(1);
            collection.Push(2);
            collection.Push(3);
            collection.Push(4);
            collection.Push(5);
            CollectionEqual(collection);
            CollectionEqualReference(ref collection, x => collection!.Clear());
        }
        {
            var collection = new ConcurrentBag<int> { 1, 2, 3, 4, 5 };

            var bin = MemoryPackSerializer.Serialize(collection);
            // not gurantees order
            MemoryPackSerializer.Deserialize<Stack<int>>(bin).Should().BeEquivalentTo(collection);
        }
        {
            var collection = new ReadOnlyCollection<int>(new[] { 1, 2, 3, 4, 5 });
            CollectionEqual(collection);
        }
        {
            var collection = new ReadOnlyObservableCollection<int>(new ObservableCollection<int> { 1, 2, 3, 4, 5 });
            CollectionEqual(collection);
        }
        {
            var collection = new BlockingCollection<int>() { 1, 2, 3, 4, 5 };
            CollectionEqual(collection);
        }
    }

    [Fact]
    public void Dictionary()
    {
        {
            var dict = new Dictionary<int, int>()
            {
                { 1, 2 }, { 3, 4 }, { 4, 5 }, { 6, 7 }, { 8, 9 }
            };

            var bin = MemoryPackSerializer.Serialize(dict);
            MemoryPackSerializer.Deserialize<Dictionary<int, int>>(bin).Should().BeEquivalentTo(dict);
        }
        {
            var dict = new SortedDictionary<int, int>()
            {
                { 1, 2 }, { 3, 4 }, { 4, 5 }, { 6, 7 }, { 8, 9 }
            };

            var bin = MemoryPackSerializer.Serialize(dict);
            MemoryPackSerializer.Deserialize<SortedDictionary<int, int>>(bin).Should().BeEquivalentTo(dict);
        }
        {
            var dict = new SortedList<int, int>()
            {
                { 1, 2 }, { 3, 4 }, { 4, 5 }, { 6, 7 }, { 8, 9 }
            };

            var bin = MemoryPackSerializer.Serialize(dict);
            MemoryPackSerializer.Deserialize<SortedList<int, int>>(bin).Should().BeEquivalentTo(dict);
        }
        {
            var dict = new ConcurrentDictionary<int, int>();
            dict.TryAdd(1, 2);
            dict.TryAdd(2, 4);
            dict.TryAdd(30, 5);
            dict.TryAdd(4, 8);

            var bin = MemoryPackSerializer.Serialize(dict);
            MemoryPackSerializer.Deserialize<ConcurrentDictionary<int, int>>(bin).Should().BeEquivalentTo(dict);
        }
    }
}
