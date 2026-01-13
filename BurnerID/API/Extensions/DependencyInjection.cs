using Infrastructure.DependencyInjection;

namespace API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddInfrastructure(config);

        // Use-case handlers (simple DI, no MediatR required for demo)
        services.AddTransient<Application.UseCases.RequestChallenge.RequestChallengeHandler>();
        services.AddTransient<Application.UseCases.AuthenticateConnection.AuthenticateConnectionHandler>();
        services.AddTransient<Application.UseCases.SendEnvelope.SendEnvelopeHandler>();

        return services;
    }
}
