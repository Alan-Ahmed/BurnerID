namespace Application.UseCases.AuthenticateConnection;

public sealed record AuthenticateConnectionCommand(
    string ConnectionId,
    string UserId,
    string ChallengeId,
    string PublicKeyBase64Url,
    string SignatureBase64Url);
