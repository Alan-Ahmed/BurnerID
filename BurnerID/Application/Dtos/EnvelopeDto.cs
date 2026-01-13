namespace Application.Dtos;

public sealed record EnvelopeDto(
    string EnvelopeId,
    string FromUserId,
    string ToUserId,
    string CiphertextBase64Url,
    string? ContentType,
    string? AlgoVersion);
