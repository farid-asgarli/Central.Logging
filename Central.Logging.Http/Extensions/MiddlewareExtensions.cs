using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Central.Logging.Http.Configuration;
using Central.Logging.Http.Middleware;

namespace Central.Logging.Http.Extensions;

/// <summary>
/// Extension methods for adding Central HTTP logging middleware to ASP.NET Core applications
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds Central HTTP logging middleware services to the dependency injection container with default configuration.
    /// Registers the HttpLoggingMiddlewareConfiguration with default settings including request/response logging,
    /// performance metrics, sensitive data masking, and standard excluded paths.
    /// </summary>
    /// <param name="services">The service collection to add HTTP logging services to</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null</exception>
    /// <remarks>
    /// Default configuration includes:
    /// <list type="bullet">
    /// <item><description>Request and response logging enabled</description></item>
    /// <item><description>Body logging enabled with 32KB size limits</description></item>
    /// <item><description>Sensitive data masking enabled</description></item>
    /// <item><description>Performance metrics logging enabled</description></item>
    /// <item><description>Standard health check and metrics endpoints excluded</description></item>
    /// <item><description>Authorization headers and sensitive fields masked</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddCentralHttpLogging();
    /// </code>
    /// </example>
    public static IServiceCollection AddCentralHttpLogging(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.Configure<HttpLoggingMiddlewareConfiguration>(config => { });
        return services;
    }

    /// <summary>
    /// Adds Central HTTP logging middleware services to the dependency injection container with custom configuration.
    /// Allows full customization of logging behavior including which requests/responses to log,
    /// body size limits, sensitive data handling, performance metrics, and path exclusions.
    /// </summary>
    /// <param name="services">The service collection to add HTTP logging services to</param>
    /// <param name="configureOptions">A delegate to configure the HTTP logging middleware options,
    /// allowing customization of logging levels, exclusions, body size limits, and sensitive field handling</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configureOptions is null</exception>
    /// <remarks>
    /// The configuration delegate allows customizing:
    /// <list type="bullet">
    /// <item><description>Request/response and body logging toggles</description></item>
    /// <item><description>Header logging settings</description></item>
    /// <item><description>Maximum request/response body sizes</description></item>
    /// <item><description>Excluded paths and sensitive fields</description></item>
    /// <item><description>Content types to log</description></item>
    /// <item><description>Performance metrics settings</description></item>
    /// <item><description>Custom request ID generation</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddCentralHttpLogging(config =>
    /// {
    ///     config.LogRequestBody = true;
    ///     config.LogResponseBody = false;
    ///     config.MaxRequestBodySize = 16384; // 16KB
    ///     config.ExcludedPaths.Add("/custom-health");
    ///     config.SensitiveFields.Add("custom-token");
    ///     config.LogPerformanceMetrics = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddCentralHttpLogging(
        this IServiceCollection services,
        Action<HttpLoggingMiddlewareConfiguration> configureOptions
    )
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Adds the Central HTTP logging middleware to the ASP.NET Core application pipeline.
    /// This middleware captures HTTP requests and responses, logs them according to the configured settings,
    /// and provides performance metrics and error tracking.
    /// </summary>
    /// <param name="app">The application builder to add the middleware to</param>
    /// <returns>The application builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when app is null</exception>
    /// <remarks>
    /// <para>
    /// This middleware should be placed early in the pipeline to capture all requests and responses.
    /// It automatically handles:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Request/response logging with configurable detail levels</description></item>
    /// <item><description>Performance timing and metrics collection</description></item>
    /// <item><description>Error logging and exception handling</description></item>
    /// <item><description>Request correlation ID generation and tracking</description></item>
    /// <item><description>Sensitive data masking for security</description></item>
    /// <item><description>Configurable path and content type filtering</description></item>
    /// </list>
    /// <para>
    /// The middleware integrates with the configured IBaseLogger instance and respects
    /// the HttpLoggingMiddlewareConfiguration settings registered via AddCentralHttpLogging.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// app.UseCentralHttpLogging();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseCentralHttpLogging(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        return app.UseMiddleware<HttpLoggingMiddleware>();
    }
}
