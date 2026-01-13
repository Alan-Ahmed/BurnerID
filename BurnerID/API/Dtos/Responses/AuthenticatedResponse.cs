namespace API.Dtos.Responses;

public sealed record AuthenticatedResponse(string UserId, bool Authenticated);
