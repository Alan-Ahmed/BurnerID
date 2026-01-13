namespace API.Dtos.Responses;

public sealed record ChallengeIssuedResponse(
    string UserId,
    string ChallengeId,
    string NonceBase64Url,
    DateTimeOffset ExpiresAtUtc);
