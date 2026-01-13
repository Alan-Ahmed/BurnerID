using Domain.Common;

namespace Domain.Models;

public sealed class Challenge
{
    public string ChallengeId { get; }
    public byte[] Nonce { get; }
    public DateTimeOffset ExpiresAt { get; }

    public Challenge(string challengeId, byte[] nonce, DateTimeOffset expiresAt)
    {
        ChallengeId = Guard.NotNullOrWhiteSpace(challengeId, nameof(challengeId));
        Nonce = Guard.NotNullOrEmpty(nonce, nameof(nonce));
        ExpiresAt = expiresAt;
    }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;
}
