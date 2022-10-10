using Microsoft.AspNetCore.Mvc.Formatters;

namespace MemoryPack.AspNetCoreMvcFormatter;

public class MemoryPackOutputFormatter : OutputFormatter
{
    const string ContentType = "application/x-memorypack";
    readonly MemoryPackSerializeOptions? options;
    readonly bool checkContentType = false;

    public MemoryPackOutputFormatter(bool checkContentType = false)
        : this(null!)
    {
        this.checkContentType = checkContentType;
    }

    public MemoryPackOutputFormatter(MemoryPackSerializeOptions options)
    {
        this.options = options;
        SupportedMediaTypes.Add(ContentType);
    }

    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        if (checkContentType)
        {
            return (context.ContentType == ContentType);
        }
        else
        {
            return true;
        }
    }

    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
        context.HttpContext.Response.ContentType = ContentType;

        if (context.Object == null)
        {
            var writer = context.HttpContext.Response.BodyWriter;
            var span = writer.GetSpan(1);
            span[0] = MemoryPackCode.NullObject;
            writer.Advance(1);
            return writer.FlushAsync().AsTask();
        }
        else
        {
            var objectType = (context.ObjectType == null || context.ObjectType == typeof(object))
                ? context.Object.GetType()
                : context.ObjectType;

            var writer = context.HttpContext.Response.BodyWriter;
            MemoryPackSerializer.Serialize(objectType, writer, context.Object, this.options);
            return writer.FlushAsync().AsTask();
        }
    }
}
