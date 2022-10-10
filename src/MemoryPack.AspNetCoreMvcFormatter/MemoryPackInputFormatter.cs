using Microsoft.AspNetCore.Mvc.Formatters;

namespace MemoryPack.AspNetCoreMvcFormatter;

public class MemoryPackInputFormatter : InputFormatter
{
    const string ContentType = "application/x-memorypack";

    public MemoryPackInputFormatter()
    {
        SupportedMediaTypes.Add(ContentType);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        var request = context.HttpContext.Request;
        var result = await MemoryPackSerializer.DeserializeAsync(context.ModelType, request.Body, context.HttpContext.RequestAborted).ConfigureAwait(false);
        return await InputFormatterResult.SuccessAsync(result).ConfigureAwait(false);
    }
}
