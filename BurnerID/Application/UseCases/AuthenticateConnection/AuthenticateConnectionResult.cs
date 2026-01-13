namespace Application.UseCases.AuthenticateConnection;

public sealed record AuthenticateConnectionResult(string UserId, bool Authenticated);
