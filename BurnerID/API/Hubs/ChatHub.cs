using API.Adapters;
using API.Dtos.Requests;
using API.Dtos.Responses;
using API.Hubs.Contracts;
using Application.Common.Abstractions;
using Application.Dtos;
using Application.UseCases.AuthenticateConnection;
using Application.UseCases.RequestChallenge;
using Application.UseCases.SendEnvelope;
using Domain.ValueObjects;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public sealed class ChatHub : Hub
{
    private readonly RequestChallengeHandler _requestChallenge;
    private readonly AuthenticateConnectionHandler _authenticate;
    private readonly SendEnvelopeHandler _sendEnvelope;
    private readonly IConnectionRegistry _connections;
    private readonly IPrivacySafeLogger _log;

    public ChatHub(
        RequestChallengeHandler requestChallenge,
        AuthenticateConnectionHandler authenticate,
        SendEnvelopeHandler sendEnvelope,
        IConnectionRegistry connections,
        IPrivacySafeLogger log)
    {
        _requestChallenge = requestChallenge;
        _authenticate = authenticate;
        _sendEnvelope = sendEnvelope;
        _connections = connections;
        _log = log;
    }

    public override async Task OnConnectedAsync()
    {
        _log.Info("WS connected connectionId={0}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _connections.UnregisterByConnectionIdAsync(Context.ConnectionId, Context.ConnectionAborted);
        _log.Info("WS disconnected connectionId={0}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<ChallengeIssuedResponse> RequestChallenge(RequestChallengeRequest req)
    {
        var result = await _requestChallenge.Handle(new RequestChallengeCommand(req.UserId), Context.ConnectionAborted);
        if (!result.IsSuccess || result.Value is null)
            throw new HubException(result.Error?.Code ?? "error");

        // Register connection to user immediately (even before auth) for demo UX
        await _connections.RegisterAsync(UserId.From(req.UserId), Context.ConnectionId, Context.ConnectionAborted);

        var c = result.Value.Challenge.Challenge;
        return new ChallengeIssuedResponse(c.UserId, c.ChallengeId, c.NonceBase64Url, c.ExpiresAtUtc);
    }

    public async Task<AuthenticatedResponse> Authenticate(AuthenticateRequest req)
    {
        var cmd = new AuthenticateConnectionCommand(
            ConnectionId: Context.ConnectionId,
            UserId: req.UserId,
            ChallengeId: req.ChallengeId,
            PublicKeyBase64Url: req.PublicKeyBase64Url,
            SignatureBase64Url: req.SignatureBase64Url);

        var result = await _authenticate.Handle(cmd, Context.ConnectionAborted);
        if (!result.IsSuccess || result.Value is null)
            throw new HubException(result.Error?.Code ?? "error");

        return new AuthenticatedResponse(result.Value.UserId, result.Value.Authenticated);
    }

    public async Task<object> SendEnvelope(SendEnvelopeRequest req)
    {
        var authed = await _connections.GetAuthenticatedUserAsync(Context.ConnectionId, Context.ConnectionAborted);
        if (authed is null)
            throw new HubException("unauthorized");

        var ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var dto = new EnvelopeDto(
            EnvelopeId: req.EnvelopeId,
            FromUserId: authed.Value.Value,
            ToUserId: req.ToUserId,
            CiphertextBase64Url: req.CiphertextBase64Url,
            ContentType: req.ContentType,
            AlgoVersion: req.AlgoVersion);

        var cmd = new SendEnvelopeCommand(Context.ConnectionId, authed.Value.Value, dto, ip);

        var result = await _sendEnvelope.Handle(cmd, Context.ConnectionAborted);
        if (!result.IsSuccess || result.Value is null)
            throw new HubException(result.Error?.Code ?? "error");

        return new { envelopeId = result.Value.EnvelopeId, delivered = result.Value.Delivered };
    }
}
