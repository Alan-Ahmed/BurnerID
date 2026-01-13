using Domain.ValueObjects;

namespace Application.Contracts;

public interface IConnectionRegistry
{
    Task RegisterAsync(UserId userId, string connectionId, CancellationToken ct);
    Task UnregisterByConnectionIdAsync(string connectionId, CancellationToken ct);

    Task<string?> GetConnectionIdAsync(UserId userId, CancellationToken ct);

    Task MarkAuthenticatedAsync(string connectionId, UserId userId, CancellationToken ct);
    Task<UserId?> GetAuthenticatedUserAsync(string connectionId, CancellationToken ct);
}
