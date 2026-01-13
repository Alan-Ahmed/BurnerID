namespace Application.Common.Options;

public sealed class RateLimitingOptions
{
    public int PerIpTokens { get; init; } = 30;
    public int PerIpRefillPerSecond { get; init; } = 1;

    public int PerUserTokens { get; init; } = 20;
    public int PerUserRefillPerSecond { get; init; } = 1;
}
