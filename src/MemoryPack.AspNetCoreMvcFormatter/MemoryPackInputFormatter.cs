using Microsoft.AspNetCore.Mvc.Formatters;

namespace MemoryPack.AspNetCoreMvcFormatter;

public class MemoryPackInputFormatter : InputFormatter
{
    public MemoryPackInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationMemoryPack);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        var request = context.HttpContext.Request;
        var result = await MemoryPackSerializer.DeserializeAsync(context.ModelType, request.Body, cancellationToken: context.HttpContext.RequestAborted).ConfigureAwait(false);
        return await InputFormatterResult.SuccessAsync(result).ConfigureAwait(false);
    }
}
