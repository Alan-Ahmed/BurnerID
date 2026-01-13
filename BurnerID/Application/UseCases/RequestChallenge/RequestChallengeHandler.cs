using Application.Common.Abstractions;
using Application.Common.Options;
using Application.Common.Results;
using Application.Contracts;
using Application.Dtos;
using Domain.ValueObjects;

namespace Application.UseCases.RequestChallenge;

public sealed class RequestChallengeHandler
{
    private readonly IChallengeStore _store;
    private readonly IClock _clock;
    private readonly SecurityOptions _security;
    private readonly IPrivacySafeLogger _log;

    public RequestChallengeHandler(
        IChallengeStore store,
        IClock clock,
        SecurityOptions security,
        IPrivacySafeLogger log)
    {
        _store = store;
        _clock = clock;
        _security = security;
        _log = log;
    }

    public async Task<Result<RequestChallengeResult>> Handle(RequestChallengeCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.UserId))
            return Result<RequestChallengeResult>.Fail(ErrorCodes.Validation, "userId is required.");

        var userId = UserId.From(cmd.UserId);

        var challenge = await _store.IssueAsync(userId, ct);

        var dto = new ChallengeDto(
            UserId: userId.Value,
            ChallengeId: challenge.ChallengeId,
            NonceBase64Url: Base64UrlEncode(challenge.Nonce),
            ExpiresAtUtc: challenge.ExpiresAt);

        _log.Info("Issued challenge for user={0} ttlSeconds={1}", userId.Value, _security.ChallengeTtlSeconds);

        return Result<RequestChallengeResult>.Ok(new RequestChallengeResult(dto));
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
