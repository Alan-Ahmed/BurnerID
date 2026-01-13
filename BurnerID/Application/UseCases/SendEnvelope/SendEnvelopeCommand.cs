using Application.Dtos;

namespace Application.UseCases.SendEnvelope;

public sealed record SendEnvelopeCommand(string ConnectionId, string SenderUserId, EnvelopeDto Envelope, string Ip);
