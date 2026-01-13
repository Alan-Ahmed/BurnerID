using Application.Common.Abstractions;

namespace API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPrivacySafeLogger _log;

    public ExceptionHandlingMiddleware(RequestDelegate next, IPrivacySafeLogger log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Unhandled exception path={0}", ctx.Request.Path.ToString());
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsJsonAsync(new { error = "internal_error" });
        }
    }
}
