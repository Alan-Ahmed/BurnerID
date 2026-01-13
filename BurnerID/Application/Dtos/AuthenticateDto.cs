namespace Application.Dtos;

public sealed record AuthenticateDto(
    string UserId,
    string ChallengeId,
    string PublicKeyBase64Url,
    string SignatureBase64Url);
