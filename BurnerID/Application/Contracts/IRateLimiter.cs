using Domain.ValueObjects;

namespace Application.Contracts;

public interface IRateLimiter
{
    Task<bool> AllowIpAsync(string ip, CancellationToken ct);
    Task<bool> AllowUserAsync(UserId userId, CancellationToken ct);
}
