namespace MemoryPack.Generator;

internal static class Extensions
{
    public static string NewLine(this IEnumerable<string> source)
    {
        return string.Join(Environment.NewLine, source);
    }
}
