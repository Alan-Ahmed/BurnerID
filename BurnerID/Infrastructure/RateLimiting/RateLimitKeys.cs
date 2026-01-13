namespace Infrastructure.RateLimiting;

public static class RateLimitKeys
{
    public static string Ip(string ip) => $"ip:{ip}";
    public static string User(string userId) => $"user:{userId}";
}
