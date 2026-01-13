using Application.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs.Filters;

public sealed class RequireAuthenticatedFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        // Allow unauthenticated calls for these methods:
        var method = invocationContext.HubMethodName;
        if (method is "RequestChallenge" or "Authenticate")
            return await next(invocationContext);

        var connId = invocationContext.Context.ConnectionId;
        var registry = invocationContext.ServiceProvider.GetRequiredService<IConnectionRegistry>();
        var authed = await registry.GetAuthenticatedUserAsync(connId, invocationContext.Context.ConnectionAborted);

        if (authed is null)
            throw new HubException("unauthorized");

        return await next(invocationContext);
    }
}
