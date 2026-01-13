namespace API.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRWithFilters(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            // keep defaults; can tune later
        });

        return services;
    }
}
