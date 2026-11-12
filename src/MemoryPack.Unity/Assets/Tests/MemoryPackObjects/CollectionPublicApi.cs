#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MemoryPack.Tests.Models
{
    [MemoryPackable]
    public partial class CollectionPublicApiItem
    {
        public int Id;
        public string? Name;
    }

    [MemoryPackable]
    public partial class CollectionPublicApiData
    {
        public List<int>? IntList;
        public List<string?>? StringList;
        public List<CollectionPublicApiItem?>? ItemList;
        public Stack<int>? IntStack;
        public Stack<long>? LongStack;
        public Stack<string?>? StringStack;
        public Stack<CollectionPublicApiItem?>? ItemStack;
        public ImmutableArray<int> IntImmutableArray;
        public ImmutableArray<string?> StringImmutableArray;
        public ImmutableArray<CollectionPublicApiItem?> ItemImmutableArray;
        public BitArray? Bits;
    }
}
