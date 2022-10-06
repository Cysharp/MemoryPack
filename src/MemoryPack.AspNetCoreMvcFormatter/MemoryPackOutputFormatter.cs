using Microsoft.AspNetCore.Mvc.Formatters;

namespace MemoryPack.AspNetCoreMvcFormatter;

public class MemoryPackOutputFormatter : OutputFormatter
{
    private const string ContentType = "application/x-memorypack";
    private readonly MemoryPackSerializeOptions? options;

    public MemoryPackOutputFormatter()
        : this(null!)
    {
    }

    public MemoryPackOutputFormatter(MemoryPackSerializeOptions options)
    {
        this.options = options;
        SupportedMediaTypes.Add(ContentType);
    }

    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
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
