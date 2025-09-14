using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Central.Logging.Abstractions;

public static class LoggingServiceHelper
{
    public static IServiceCollection RegisterCommonServices(
        IServiceCollection services,
        CentralLoggingConfiguration configuration,
        LoggerConfiguration loggerConfig
    )
    {
        services.RemoveAll<ILoggerFactory>().RemoveAll(typeof(ILogger<>));

        Log.Logger = loggerConfig.CreateLogger();

        return services
            .AddSingleton<ILoggerFactory>(provider => new SerilogLoggerFactory(
                Log.Logger,
                dispose: true
            ))
            .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
            .AddSingleton(configuration)
            .AddSingleton<IBaseLogger>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(nameof(BaseLogger));
                return new BaseLogger(logger);
            });
    }
}
