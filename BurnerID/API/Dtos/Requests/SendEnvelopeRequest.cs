namespace API.Dtos.Requests;

public sealed record SendEnvelopeRequest(
    string EnvelopeId,
    string ToUserId,
    string CiphertextBase64Url,
    string? ContentType,
    string? AlgoVersion);
