using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Benchmark.Micro;

public class Utf8Decoding
{
    byte[] utf8bytes;
    int utf8length;
    int utf16length;

    public Utf8Decoding()
    {
        // Japanese Hiragana
        var text = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん";
        utf8bytes = Encoding.UTF8.GetBytes(text);
        utf8length = utf8bytes.Length;
        utf16length = text.Length;
    }

    [Benchmark]
    public string UTF8GetString()
    {
        return Encoding.UTF8.GetString(utf8bytes);
    }

    [Benchmark]
    public string Utf16LengthUtf8ToUtf16()
    {
        return string.Create(utf16length, utf8bytes, static (dest, source) =>
        {
            Utf8.ToUtf16(source, dest, out var read, out var written);
        });
    }
}
