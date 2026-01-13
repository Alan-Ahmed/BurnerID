using Application.Contracts;
using Domain.Models;
using Domain.ValueObjects;
using Microsoft.AspNetCore.SignalR;
using API.Hubs;
using API.Hubs.Contracts;

namespace API.Adapters;

public sealed class SignalREnvelopeRouter : IEnvelopeRouter
{
    private readonly IHubContext<ChatHub> _hub;
    private readonly IConnectionRegistry _connections;

    public SignalREnvelopeRouter(IHubContext<ChatHub> hub, IConnectionRegistry connections)
    {
        _hub = hub;
        _connections = connections;
    }

    public async Task DeliverAsync(UserId recipient, Envelope envelope, CancellationToken ct)
    {
        var connId = await _connections.GetConnectionIdAsync(recipient, ct);

        // online-only delivery: if recipient not connected, do nothing
        if (string.IsNullOrWhiteSpace(connId))
            return;

        var payload = new
        {
            envelopeId = envelope.EnvelopeId,
            fromUserId = envelope.From.Value,
            toUserId = envelope.To.Value,
            ciphertextBase64Url = Convert.ToBase64String(envelope.Ciphertext).TrimEnd('=').Replace('+', '-').Replace('/', '_'),
            contentType = envelope.ContentType,
            algoVersion = envelope.AlgoVersion,
            createdAtUtc = envelope.CreatedAt
        };

        await _hub.Clients.Client(connId).SendAsync(ClientEvents.ReceiveEnvelope, payload, ct);
    }
}
