namespace Infrastructure.Stores.Expiration;

public sealed class ExpiringItem<T>
{
    public T Value { get; }
    public DateTimeOffset ExpiresAt { get; }

    public ExpiringItem(T value, DateTimeOffset expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;
}
