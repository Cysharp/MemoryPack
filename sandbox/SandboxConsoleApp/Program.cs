using MemoryPack;

ConsoleAppFramework.ConsoleApp.Run<StandardRunner>(args);

public class StandardRunner : ConsoleAppBase
{
    [RootCommand]
    public void Run()
    {
        var bin = MemoryPackSerializer.Serialize(9999);
        foreach (var item in bin)
        {
            Console.WriteLine(item);
        }
    }
}