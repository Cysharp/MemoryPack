using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MemoryPack.AspNetCoreMvcFormatter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace MemoryPack.Tests;

public class MvcOutputFormatterTest
{
    [Theory]
    [InlineData(typeof(MvcNullModel))]
    [InlineData(typeof(string))]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(List<int>))]
    [InlineData(typeof(Dictionary<string, int>))]
    public async Task NullUsesDeclaredType(Type type)
    {
        var httpContext = new DefaultHttpContext();
        using var body = new MemoryStream();
        httpContext.Response.Body = body;
        var context = new OutputFormatterWriteContext(httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding), type, null);

        await new MemoryPackOutputFormatter().WriteResponseBodyAsync(context);

        body.ToArray().Should().Equal(MemoryPackSerializer.Serialize(type, (object?)null));
        MemoryPackSerializer.Deserialize(type, body.ToArray()).Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(typeof(object))]
    public async Task NullWithoutDeclaredType(Type? type)
    {
        var httpContext = new DefaultHttpContext();
        using var body = new MemoryStream();
        httpContext.Response.Body = body;
        var context = new OutputFormatterWriteContext(httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding), type, null);

        await new MemoryPackOutputFormatter().WriteResponseBodyAsync(context);

        body.ToArray().Should().Equal(new byte[] { MemoryPackCode.NullObject });
    }
}

[MemoryPackable]
public partial class MvcNullModel
{
    public int Value { get; set; }
}
