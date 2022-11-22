using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

ConsoleApp.Run<Runner>(args);

public class Runner : ConsoleAppBase
{
    [RootCommand]
    public void CopyToUnity([Option(0)] string directory)
    {
        var replaceSet = new Dictionary<string, string>
        {
            // to C# 9
            {"scoped ref", "ref" },
            {"scoped in", "in" },
            {"scoped Span", "Span" },
            {"scoped ReadOnlySpan", "ReadOnlySpan" },
            {"file static", "internal static" },
            // remove <TBufferWriter> for IL2CPP safety
            {"Serialize<TBufferWriter>", "Serialize" },
            {"<TBufferWriter>", "" },
            {"<TBufferWriter, ", "<" },
            {", TBufferWriter>", ">" },
            {"where TBufferWriter : class, IBufferWriter<byte>;", ";" },
            {"where TBufferWriter : class, IBufferWriter<byte>", "" },
            {"where TBufferWriter : IBufferWriter<byte>", "" },
            {"MemoryPackWriter<ReusableLinkedArrayBufferWriter>", "MemoryPackWriter" },
            {"TBufferWriter", "IBufferWriter<byte>" },
            {"new MemoryPackWriter(ref bufferWriter", "new MemoryPackWriter(ref Unsafe.As<ReusableLinkedArrayBufferWriter, IBufferWriter<byte>>(ref bufferWriter)" },
            {"new MemoryPackWriter(ref tempBuffer", "new MemoryPackWriter(ref Unsafe.As<ReusableLinkedArrayBufferWriter, IBufferWriter<byte>>(ref tempBuffer)" },
            {"new MemoryPackWriter(ref state.BufferWriter", "new MemoryPackWriter(ref Unsafe.As<ReusableLinkedArrayBufferWriter, IBufferWriter<byte>>(ref state.BufferWriter)" },
            {"new MemoryPackWriter<BrotliCompressor>(ref compressor", "new MemoryPackWriter(ref Unsafe.As<BrotliCompressor, IBufferWriter<byte>>(ref compressor)" },
        };

        System.Console.WriteLine("Start to modify code.");
        var noBomUtf8 = new UTF8Encoding(false);

        foreach (var path in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);

            // replace
            foreach (var item in replaceSet)
            {
                text = text.Replace(item.Key, item.Value);
            }

            // add namespace
            if (Regex.Count(text, "namespace") == 1)
            {
                text = Regex.Replace(text, "(namespace.+);", "$1 {");
                text += "\r\n}";
            }

            // add implicit global using and nullable enable
            text = """
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

""" + text;

            File.WriteAllText(path, text, noBomUtf8);
        }

        System.Console.WriteLine("Copy complete.");
    }
}
