using System.Collections.Generic;

namespace MemoryPack.Tests;

public class GenericCollectionRegistrationTest
{
    [Fact]
    public void Collection()
    {
        MemoryPackFormatterProvider.RegisterCollection(typeof(RegisteredList<>));

        var value = new RegisteredList<int> { 1, 10, 100 };
        var bytes = MemoryPackSerializer.Serialize(value);
        MemoryPackSerializer.Deserialize<RegisteredList<int>>(bytes).Should().Equal(value);

        var strings = new RegisteredList<string?> { "one", null, "three" };
        var nonGenericBytes = MemoryPackSerializer.Serialize(typeof(RegisteredList<string?>), strings);
        ((RegisteredList<string?>)MemoryPackSerializer.Deserialize(typeof(RegisteredList<string?>), nonGenericBytes)!).Should().Equal(strings);
    }

    [Fact]
    public void Set()
    {
        MemoryPackFormatterProvider.RegisterSet(typeof(RegisteredSet<>));

        var value = new RegisteredSet<int> { 1, 10, 100 };
        var bytes = MemoryPackSerializer.Serialize(value);
        MemoryPackSerializer.Deserialize<RegisteredSet<int>>(bytes).Should().BeEquivalentTo(value);

        var strings = new RegisteredSet<string?> { "one", null, "three" };
        var nonGenericBytes = MemoryPackSerializer.Serialize(typeof(RegisteredSet<string?>), strings);
        ((RegisteredSet<string?>)MemoryPackSerializer.Deserialize(typeof(RegisteredSet<string?>), nonGenericBytes)!).Should().BeEquivalentTo(strings);
    }

    [Fact]
    public void Dictionary()
    {
        MemoryPackFormatterProvider.RegisterDictionary(typeof(RegisteredDictionary<,>));

        var value = new RegisteredDictionary<int, string?> { { 1, "one" }, { 2, null } };
        var bytes = MemoryPackSerializer.Serialize(value);
        MemoryPackSerializer.Deserialize<RegisteredDictionary<int, string?>>(bytes).Should().Equal(value);

        var strings = new RegisteredDictionary<string, int> { { "one", 1 }, { "two", 2 } };
        var nonGenericBytes = MemoryPackSerializer.Serialize(typeof(RegisteredDictionary<string, int>), strings);
        ((RegisteredDictionary<string, int>)MemoryPackSerializer.Deserialize(typeof(RegisteredDictionary<string, int>), nonGenericBytes)!).Should().Equal(strings);
    }
}

public class RegisteredList<T> : List<T>
{
}

public class RegisteredSet<T> : HashSet<T>
{
}

public class RegisteredDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    where TKey : notnull
{
}
