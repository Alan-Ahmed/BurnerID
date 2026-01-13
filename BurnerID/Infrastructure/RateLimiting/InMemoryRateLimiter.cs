using System.Collections.Concurrent;
using Application.Common.Abstractions;
using Application.Common.Options;
using Application.Contracts;
using Domain.ValueObjects;

namespace Infrastructure.RateLimiting;

public sealed class InMemoryRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly IClock _clock;
    private readonly RateLimitingOptions _opt;

    public InMemoryRateLimiter(IClock clock, RateLimitingOptions opt)
    {
        _clock = clock;
        _opt = opt;
    }

    public Task<bool> AllowIpAsync(string ip, CancellationToken ct)
    {
        var key = RateLimitKeys.Ip(ip);
        var now = _clock.UtcNow;
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_opt.PerIpTokens, _opt.PerIpRefillPerSecond, now));
        var ok = bucket.TryConsume(1, now);
        return Task.FromResult(ok);
    }

    public Task<bool> AllowUserAsync(UserId userId, CancellationToken ct)
    {
        var key = RateLimitKeys.User(userId.Value);
        var now = _clock.UtcNow;
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_opt.PerUserTokens, _opt.PerUserRefillPerSecond, now));
        var ok = bucket.TryConsume(1, now);
        return Task.FromResult(ok);
    }
}
