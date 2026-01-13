using Application.Contracts;
using Microsoft.AspNetCore.SignalR;
using System.Threading.RateLimiting;

namespace API.Hubs.Filters;

public sealed class RateLimitSendEnvelopeFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        if (invocationContext.HubMethodName != "SendEnvelope")
            return await next(invocationContext);

        var limiter = invocationContext.ServiceProvider.GetRequiredService<IRateLimiter>();

        var http = invocationContext.Context.GetHttpContext();
        var ip = http?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Per-IP gating here; per-user gating happens in UseCase too.
        var okIp = await limiter.AllowIpAsync(ip, invocationContext.Context.ConnectionAborted);
        if (!okIp)
            throw new HubException("rate_limited");

        return await next(invocationContext);
    }
}
