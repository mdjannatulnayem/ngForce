namespace ngForce.Authentication;

public class ApiEndpointAuthFilter : IEndpointFilter
{
    private readonly IConfiguration _configuration;

    public ApiEndpointAuthFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName,out var extractedApiKey))
        {
            return new UnauthorizedHttpObjectResult("Api key missing");
        }
        var apiKey = _configuration.GetValue<string>(AuthConstants.ApiKeySectionName);
        if (!apiKey.Equals(extractedApiKey))
        {
            return new UnauthorizedHttpObjectResult("Invalid Api key");
        }
        return await next(context);
    }
}

