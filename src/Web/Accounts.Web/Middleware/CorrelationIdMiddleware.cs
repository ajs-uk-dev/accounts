namespace Accounts.Web.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var id = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString("N");
        context.TraceIdentifier = id;
        context.Response.Headers[HeaderName] = id;
        await _next(context);
    }
}
