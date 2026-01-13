using System.Diagnostics;

namespace API.Middleware;

public sealed class CorrelationIdMiddleware
{
    private const string Header = "x-correlation-id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        var id = ctx.Request.Headers.TryGetValue(Header, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString()
            : Activity.Current?.Id ?? Guid.NewGuid().ToString("N");

        ctx.Response.Headers[Header] = id;
        await _next(ctx);
    }
}
