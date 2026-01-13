using Application.Common.Abstractions;
using Application.Common.Options;
using Application.Contracts;
using Infrastructure.Connections;
using Infrastructure.Crypto;
using Infrastructure.Logging;
using Infrastructure.RateLimiting;
using Infrastructure.Stores;
using Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // options (simple bind)
        var rate = new RateLimitingOptions();
        config.GetSection("RateLimiting").Bind(rate);
        services.AddSingleton(rate);

        var sec = new SecurityOptions();
        config.GetSection("Security").Bind(sec);
        services.AddSingleton(sec);

        // core abstractions
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPrivacySafeLogger, PrivacySafeLogger>();

        // stores + registries
        services.AddSingleton<IChallengeStore, InMemoryChallengeStore>();
        services.AddSingleton<IConnectionRegistry, InMemoryConnectionRegistry>();
        services.AddSingleton<IRateLimiter, InMemoryRateLimiter>();

        // crypto
        services.AddSingleton<ICryptoVerifier, Ed25519CryptoVerifier>();

        return services;
    }
}
