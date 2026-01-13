using Domain.Models;
using Domain.ValueObjects;

namespace Application.Contracts;

public interface IChallengeStore
{
    Task<Challenge> IssueAsync(UserId userId, CancellationToken ct);
    Task<Challenge?> GetAsync(UserId userId, string challengeId, CancellationToken ct);

    Task<bool> ConsumeAsync(UserId userId, string challengeId, CancellationToken ct);
}
