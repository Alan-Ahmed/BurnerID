namespace Infrastructure.RateLimiting;

public sealed class TokenBucket
{
    private readonly int _capacity;
    private readonly int _refillPerSecond;

    private double _tokens;
    private DateTimeOffset _last;

    public TokenBucket(int capacity, int refillPerSecond, DateTimeOffset now)
    {
        _capacity = Math.Max(1, capacity);
        _refillPerSecond = Math.Max(0, refillPerSecond);
        _tokens = _capacity;
        _last = now;
    }

    public bool TryConsume(int amount, DateTimeOffset now)
    {
        Refill(now);
        if (_tokens >= amount)
        {
            _tokens -= amount;
            return true;
        }
        return false;
    }

    private void Refill(DateTimeOffset now)
    {
        var seconds = (now - _last).TotalSeconds;
        if (seconds <= 0) return;

        _tokens = Math.Min(_capacity, _tokens + seconds * _refillPerSecond);
        _last = now;
    }
}
