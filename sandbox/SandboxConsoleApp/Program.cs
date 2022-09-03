using MemoryPack;
using MemoryPack.Formatters;
using System.Buffers;

ConsoleAppFramework.ConsoleApp.Run<StandardRunner>(args);

public class StandardRunner : ConsoleAppBase
{
    [RootCommand]
    public void Run()
    {
        CompositeMemoryPackProvider.Default.Register(new VersionFormatter());

        MemoryPackSerializer.DefaultProvider = CompositeMemoryPackProvider.Default;

        int? v = 88;
        //v = null;
        var bytes = MemoryPackSerializer.Serialize(v);

        //var bytes = MemoryPackSerializer.Serialize(new Version(10, 20, 30, 40));

        foreach (var item in bytes)
        {
            Console.WriteLine(item);
        }

        //var version = MemoryPackSerializer.Deserialize<Version>(bytes);

        //Console.WriteLine(version!.ToString());
    }
}
