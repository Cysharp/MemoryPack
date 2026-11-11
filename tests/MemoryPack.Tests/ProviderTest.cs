#if NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryPack.Tests;

public class ProviderTest
{
    [Fact]
    public void AnonymousType()
    {
        Assert.Throws<MemoryPackSerializationException>(() =>
        MemoryPackSerializer.Serialize(new { foo = 10, bar = "tako" })).Message.Should().Contain("anonymous type");
    }

    [Fact]

    public void RegisterAs()
    {
        MemoryPackFormatterProvider.RegisterGenericType(typeof(CustomType<>), typeof(CustomTypeFormatter<>));

        var t = new CustomType<int>() { Value = 9999 };
        var bin = MemoryPackSerializer.Serialize(t);
        MemoryPackSerializer.Deserialize<CustomType<int>>(bin)!.Value.Should().Be(9999);
    }

    [Fact]
    public void RegisterPublishesFormatterBeforeConcurrentFirstAccess()
    {
        var isRegisteredDefinition = typeof(MemoryPackFormatterProvider).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Single(x => x.Name == "IsRegistered" && x.IsGenericMethodDefinition);
        var getFormatterDefinition = typeof(MemoryPackFormatterProvider).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Single(x => x.Name == "GetFormatter" && x.IsGenericMethodDefinition);
        var assembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
            new System.Reflection.AssemblyName("MemoryPackRegistrationRace"), System.Reflection.Emit.AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("MemoryPackRegistrationRace");
        var failures = new System.Collections.Concurrent.ConcurrentQueue<string>();

        for (var i = 0; i < 128; i++)
        {
            var marker = module.DefineType($"Marker{i}", System.Reflection.TypeAttributes.Public).CreateType()!;
            var type = typeof(RaceTarget<>).MakeGenericType(marker);
            var isRegistered = isRegisteredDefinition.MakeGenericMethod(type);
            var getFormatter = getFormatterDefinition.MakeGenericMethod(type);
            var start = new System.Threading.ManualResetEventSlim();
            var readers = Enumerable.Range(0, 8).Select(_ => new System.Threading.Thread(() =>
            {
                start.Wait();
                var deadline = System.Diagnostics.Stopwatch.StartNew();
                while (!(bool)isRegistered.Invoke(null, null)! && deadline.Elapsed < TimeSpan.FromSeconds(5))
                    System.Threading.Thread.SpinWait(1);
                if (!(bool)isRegistered.Invoke(null, null)!)
                {
                    failures.Enqueue("registration did not publish its state");
                    return;
                }
                try
                {
                    if (getFormatter.Invoke(null, null) is null)
                        failures.Enqueue("formatter cache was null after registration became visible");
                }
                catch (Exception ex)
                {
                    failures.Enqueue(ex.GetBaseException().GetType().Name);
                }
            }) { IsBackground = true }).ToArray();
            var writer = new System.Threading.Thread(() =>
            {
                start.Wait();
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }) { IsBackground = true };

            foreach (var reader in readers) reader.Start();
            writer.Start();
            start.Set();
            Assert.True(writer.Join(TimeSpan.FromSeconds(5)), "registration did not complete");
            foreach (var reader in readers)
                Assert.True(reader.Join(TimeSpan.FromSeconds(5)), "formatter lookup did not complete");
        }

        Assert.Empty(failures);
    }

    //[Fact]
    //public void RegisterGenericTest()
    //{
    //    MemoryPackFormatterProvider.RegisterCollection<MyList, int>();

    //    var list = new MyList { 1, 10, 100 };


    //    var bin = MemoryPackSerializer.Serialize(list);
    //    var foo = MemoryPackSerializer.Deserialize<MyList>(bin);
    //}

}

public class CustomType<T>
{
    public T? Value { get; set; }
}

public class CustomTypeFormatter<T> : MemoryPackFormatter<CustomType<T?>>
    where T : notnull
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref CustomType<T?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteValue<T>(value.Value);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref CustomType<T?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }
        if (count != 1) MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);

        value = new CustomType<T?> { Value = reader.ReadValue<T>() };
    }
}

public class MyList : List<int>
{
}

#endif


internal sealed class RaceTarget<TTag> : IMemoryPackFormatterRegister
{
    static RaceTarget()
    {
        RegisterFormatter();
    }

    public static void RegisterFormatter()
    {
        MemoryPackFormatterProvider.Register(new RaceTargetFormatter<TTag>());
    }
}

internal sealed class RaceTargetFormatter<TTag> : MemoryPackFormatter<RaceTarget<TTag>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref RaceTarget<TTag>? value)
    {
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref RaceTarget<TTag>? value)
    {
        value = new RaceTarget<TTag>();
    }
}
