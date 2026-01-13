using Application.Common.Abstractions;
using Application.Common.Results;
using Application.Contracts;
using Application.Mapping;
using Domain.ValueObjects;

namespace Application.UseCases.SendEnvelope;

public sealed class SendEnvelopeHandler
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IEnvelopeRouter _router;
    private readonly IConnectionRegistry _connections;
    private readonly IClock _clock;
    private readonly IPrivacySafeLogger _log;

    public SendEnvelopeHandler(
        IRateLimiter rateLimiter,
        IEnvelopeRouter router,
        IConnectionRegistry connections,
        IClock clock,
        IPrivacySafeLogger log)
    {
        _rateLimiter = rateLimiter;
        _router = router;
        _connections = connections;
        _clock = clock;
        _log = log;
    }

    public async Task<Result<SendEnvelopeResult>> Handle(SendEnvelopeCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.ConnectionId) ||
            string.IsNullOrWhiteSpace(cmd.SenderUserId) ||
            string.IsNullOrWhiteSpace(cmd.Ip))
            return Result<SendEnvelopeResult>.Fail(ErrorCodes.Validation, "missing fields.");

        var authedUser = await _connections.GetAuthenticatedUserAsync(cmd.ConnectionId, ct);
        if (authedUser is null || authedUser.Value.Value != cmd.SenderUserId)
            return Result<SendEnvelopeResult>.Fail(ErrorCodes.Unauthorized, "not authenticated.");

        var sender = UserId.From(cmd.SenderUserId);

        if (!await _rateLimiter.AllowIpAsync(cmd.Ip, ct))
            return Result<SendEnvelopeResult>.Fail(ErrorCodes.RateLimited, "rate limited (ip).");

        if (!await _rateLimiter.AllowUserAsync(sender, ct))
            return Result<SendEnvelopeResult>.Fail(ErrorCodes.RateLimited, "rate limited (user).");

        var envelope = cmd.Envelope.ToDomain(_clock.UtcNow);

        if (envelope.From.Value != sender.Value)
            return Result<SendEnvelopeResult>.Fail(ErrorCodes.Validation, "fromUserId mismatch.");

        await _router.DeliverAsync(envelope.To, envelope, ct);

        _log.Info("Routed envelope id={0} sizeBytes={1}", envelope.EnvelopeId, envelope.PayloadSizeBytes);

        return Result<SendEnvelopeResult>.Ok(new SendEnvelopeResult(envelope.EnvelopeId, true));
    }
}
