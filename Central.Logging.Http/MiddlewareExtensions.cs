using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Central.Logging.Http;

/// <summary>
/// Extension methods for adding HTTP logging middleware
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds HTTP logging middleware services to the DI container
    /// </summary>
    public static IServiceCollection AddHttpCentralLogging(this IServiceCollection services)
    {
        services.Configure<HttpLoggingMiddlewareConfiguration>(config => { });
        return services;
    }

    /// <summary>
    /// Adds HTTP logging middleware services with custom configuration
    /// </summary>
    public static IServiceCollection AddHttpCentralLogging(
        this IServiceCollection services,
        Action<HttpLoggingMiddlewareConfiguration> configureOptions
    )
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Uses the HTTP logging middleware in the application pipeline
    /// </summary>
    public static IApplicationBuilder UseCentralHttpLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<HttpLoggingMiddleware>();
    }
}
