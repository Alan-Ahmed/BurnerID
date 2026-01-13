using System.Collections.Concurrent;
using System.Security.Cryptography;
using Application.Common.Abstractions;
using Application.Common.Options;
using Application.Contracts;
using Domain.Models;
using Domain.ValueObjects;
using Infrastructure.Stores.Expiration;

namespace Infrastructure.Stores;

public sealed class InMemoryChallengeStore : IChallengeStore
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ExpiringItem<Challenge>>> _byUser
        = new();

    private readonly IClock _clock;
    private readonly SecurityOptions _security;

    public InMemoryChallengeStore(IClock clock, SecurityOptions security)
    {
        _clock = clock;
        _security = security;
    }

    public Task<Challenge> IssueAsync(UserId userId, CancellationToken ct)
    {
        var nonce = RandomNumberGenerator.GetBytes(32);
        var challengeId = Guid.NewGuid().ToString("N");
        var expires = _clock.UtcNow.AddSeconds(_security.ChallengeTtlSeconds);

        var c = new Challenge(challengeId, nonce, expires);

        var userMap = _byUser.GetOrAdd(userId.Value, _ => new ConcurrentDictionary<string, ExpiringItem<Challenge>>());
        userMap[challengeId] = new ExpiringItem<Challenge>(c, expires);

        CleanupExpired(userMap);

        return Task.FromResult(c);
    }

    public Task<Challenge?> GetAsync(UserId userId, string challengeId, CancellationToken ct)
    {
        if (!_byUser.TryGetValue(userId.Value, out var userMap))
            return Task.FromResult<Challenge?>(null);

        CleanupExpired(userMap);

        if (!userMap.TryGetValue(challengeId, out var item))
            return Task.FromResult<Challenge?>(null);

        if (item.IsExpired(_clock.UtcNow))
        {
            userMap.TryRemove(challengeId, out _);
            return Task.FromResult<Challenge?>(null);
        }

        return Task.FromResult<Challenge?>(item.Value);
    }

    public Task<bool> ConsumeAsync(UserId userId, string challengeId, CancellationToken ct)
    {
        if (!_byUser.TryGetValue(userId.Value, out var userMap))
            return Task.FromResult(false);

        CleanupExpired(userMap);

        return Task.FromResult(userMap.TryRemove(challengeId, out _));
    }

    private void CleanupExpired(ConcurrentDictionary<string, ExpiringItem<Challenge>> userMap)
    {
        var now = _clock.UtcNow;
        foreach (var kv in userMap)
        {
            if (kv.Value.IsExpired(now))
                userMap.TryRemove(kv.Key, out _);
        }
    }
}
