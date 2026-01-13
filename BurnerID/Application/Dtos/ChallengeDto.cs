namespace Application.Dtos;

public sealed record ChallengeDto(
    string UserId,
    string ChallengeId,
    string NonceBase64Url,
    DateTimeOffset ExpiresAtUtc);
