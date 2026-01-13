namespace Application.UseCases.SendEnvelope;

public sealed record SendEnvelopeResult(string EnvelopeId, bool Delivered);
