namespace API.Dtos.Requests;

public sealed record AuthenticateRequest(
    string UserId,
    string ChallengeId,
    string PublicKeyBase64Url,
    string SignatureBase64Url);
