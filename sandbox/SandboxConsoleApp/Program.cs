using MemoryPack;
using System.Buffers;

ConsoleAppFramework.ConsoleApp.Run<StandardRunner>(args);

public class StandardRunner : ConsoleAppBase
{
    [RootCommand]
    public void Run()
    {
        var buf1 = ArrayPool<byte>.Shared.Rent(10);
        ArrayPool<byte>.Shared.Return(buf1);


        ArrayPool<byte>.Shared.Return(new byte[65536 - 1]);


        //var bin = MemoryPackSerializer.Serialize(9999);
        //foreach (var item in bin)
        //{
        //    Console.WriteLine(item);
        //}
    }
}
