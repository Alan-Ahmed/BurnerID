using System.Collections.Concurrent;
using Application.Contracts;
using Domain.ValueObjects;

namespace Infrastructure.Connections;

public sealed class InMemoryConnectionRegistry : IConnectionRegistry
{
    private readonly ConcurrentDictionary<string, string> _userToConn = new(); // userId -> connectionId
    private readonly ConcurrentDictionary<string, string> _connToUser = new(); // connectionId -> userId
    private readonly ConcurrentDictionary<string, string> _authedConnToUser = new(); // connectionId -> userId

    public Task RegisterAsync(UserId userId, string connectionId, CancellationToken ct)
    {
        _userToConn[userId.Value] = connectionId;
        _connToUser[connectionId] = userId.Value;
        return Task.CompletedTask;
    }

    public Task UnregisterByConnectionIdAsync(string connectionId, CancellationToken ct)
    {
        if (_connToUser.TryRemove(connectionId, out var userId))
        {
            _userToConn.TryRemove(userId, out _);
        }

        _authedConnToUser.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }

    public Task<string?> GetConnectionIdAsync(UserId userId, CancellationToken ct)
    {
        return Task.FromResult(_userToConn.TryGetValue(userId.Value, out var conn) ? conn : null);
    }

    public Task MarkAuthenticatedAsync(string connectionId, UserId userId, CancellationToken ct)
    {
        _authedConnToUser[connectionId] = userId.Value;
        return Task.CompletedTask;
    }

    public Task<UserId?> GetAuthenticatedUserAsync(string connectionId, CancellationToken ct)
    {
        if (_authedConnToUser.TryGetValue(connectionId, out var user))
            return Task.FromResult<UserId?>(UserId.From(user));

        return Task.FromResult<UserId?>(null);
    }
}
