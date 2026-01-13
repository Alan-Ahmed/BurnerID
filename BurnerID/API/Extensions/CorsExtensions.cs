namespace API.Extensions;

public static class CorsExtensions
{
    private const string PolicyName = "LocalDevCors";

    public static IServiceCollection AddLocalDevCors(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, builder =>
            {
                builder
                    .WithOrigins(origins.Length == 0 ? new[] { "http://localhost:3000" } : origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseLocalDevCors(this IApplicationBuilder app)
        => app.UseCors(PolicyName);
}
