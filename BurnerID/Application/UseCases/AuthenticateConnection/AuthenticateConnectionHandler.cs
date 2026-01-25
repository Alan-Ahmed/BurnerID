using Application.Common.Abstractions;
using Application.Common.Results;
using Application.Contracts;
using Domain.ValueObjects;

namespace Application.UseCases.AuthenticateConnection;

public sealed class AuthenticateConnectionHandler
{
    private readonly IChallengeStore _challengeStore;
    private readonly IConnectionRegistry _connections;
    private readonly ICryptoVerifier _crypto;
    private readonly IClock _clock;
    private readonly IPrivacySafeLogger _log;

    public AuthenticateConnectionHandler(
        IChallengeStore challengeStore,
        IConnectionRegistry connections,
        ICryptoVerifier crypto,
        IClock clock,
        IPrivacySafeLogger log)
    {
        _challengeStore = challengeStore;
        _connections = connections;
        _crypto = crypto;
        _clock = clock;
        _log = log;
    }

    public async Task<Result<AuthenticateConnectionResult>> Handle(AuthenticateConnectionCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.ConnectionId))
            return Result<AuthenticateConnectionResult>.Fail(ErrorCodes.Validation, "connectionId is required.");

        if (string.IsNullOrWhiteSpace(cmd.UserId) ||
            string.IsNullOrWhiteSpace(cmd.ChallengeId) ||
            string.IsNullOrWhiteSpace(cmd.PublicKeyBase64Url) ||
            string.IsNullOrWhiteSpace(cmd.SignatureBase64Url))
        {
            return Result<AuthenticateConnectionResult>.Fail(ErrorCodes.Validation, "missing fields.");
        }

        var userId = UserId.From(cmd.UserId);

        var challenge = await _challengeStore.GetAsync(userId, cmd.ChallengeId, ct);
        if (challenge is null)
            return Result<AuthenticateConnectionResult>.Fail(ErrorCodes.NotFound, "challenge not found.");

        if (challenge.IsExpired(_clock.UtcNow))
            return Result<AuthenticateConnectionResult>.Fail(ErrorCodes.Unauthorized, "challenge expired.");

        // Avkoda strängarna till bytes
        var pubKey = Base64UrlDecode(cmd.PublicKeyBase64Url);
        var sig = Base64UrlDecode(cmd.SignatureBase64Url);

        // --- HÄR VAR FELET ---
        // Tidigare: _crypto.VerifyEd25519(pubKey, challenge.Nonce, sig);
        // Rätt ordning för din Verifier är: (Message, Signature, PublicKey)

        var ok = _crypto.VerifyEd25519(challenge.Nonce, sig, pubKey);

        if (!ok)
            return Result<AuthenticateConnectionResult>.Fail(ErrorCodes.Unauthorized, "invalid signature.");

        var consumed = await _challengeStore.ConsumeAsync(userId, cmd.ChallengeId, ct);
        if (!consumed)
            return Result<AuthenticateConnectionResult>.Fail(ErrorCodes.Conflict, "challenge already consumed.");

        await _connections.MarkAuthenticatedAsync(cmd.ConnectionId, userId, ct);

        _log.Info("Authenticated connection={0} user={1}", cmd.ConnectionId, userId.Value);

        return Result<AuthenticateConnectionResult>.Ok(new AuthenticateConnectionResult(userId.Value, true));
    }

    private static byte[] Base64UrlDecode(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}