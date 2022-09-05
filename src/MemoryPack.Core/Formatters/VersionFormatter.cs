using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters;

public sealed class VersionFormatter : IMemoryPackFormatter<Version>
{
    [ModuleInitializer]
    internal static void RegisterFormatter() => MemoryPackFormatterProvider.Register(new VersionFormatter());

    // Serialize as [Major, Minor, Build, Revision]

    public void Serialize<TBufferWriter>(ref SerializationContext<TBufferWriter> context, ref Version? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            context.WriteNullObjectHeader();
            return;
        }

        ref var spanRef = ref context.GetSpanReference(17); // header + int * 4

        Unsafe.WriteUnaligned(ref spanRef, 4);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref spanRef, 1), value.Major);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref spanRef, 5), value.Minor);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref spanRef, 9), value.Build);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref spanRef, 13), value.Revision);

        context.Advance(17);
    }

    public void Deserialize(ref DeserializationContext context, ref Version? value)
    {
        if (!context.TryReadPropertyCount(out var count))
        {
            value = null;
            return;
        }

        if (count != 4) ThrowHelpers.InvalidPropertyCount(4, count);

        ref var spanRef = ref context.GetSpanReference(16);

        var major = Unsafe.ReadUnaligned<int>(ref spanRef);
        var minor = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref spanRef, 4));
        var build = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref spanRef, 8));
        var revision = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref spanRef, 12));

        value = new Version(major, minor, build, revision);

        context.Advance(16);
    }
}
