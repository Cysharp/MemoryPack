using Microsoft.Net.Http.Headers;

namespace MemoryPack.AspNetCoreMvcFormatter;

public static class MediaTypeHeaderValues
{
    public static readonly MediaTypeHeaderValue ApplicationMemoryPack =
        MediaTypeHeaderValue.Parse("application/x-memorypack");
}
