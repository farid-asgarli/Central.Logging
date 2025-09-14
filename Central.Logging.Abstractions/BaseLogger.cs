using Microsoft.Extensions.Logging;

namespace Central.Logging.Abstractions;

/// <summary>
/// Implementation of Extended logger using Serilog
/// </summary>
public class BaseLogger(ILogger logger) : IBaseLogger
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void Log(LogLevel level, string category, string message, params object[] args)
    {
        Log(level, category, null, message, args);
    }

    public void Log(
        LogLevel level,
        string category,
        Exception? exception,
        string message,
        params object[] args
    )
    {
        using var scope = BeginCategoryScope(category);

        switch (level)
        {
            case LogLevel.Trace:
                _logger.LogTrace(exception, message, args);
                break;
            case LogLevel.Debug:
                _logger.LogDebug(exception, message, args);
                break;
            case LogLevel.Information:
                _logger.LogInformation(exception, message, args);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(exception, message, args);
                break;
            case LogLevel.Error:
                _logger.LogError(exception, message, args);
                break;
            case LogLevel.Critical:
                _logger.LogCritical(exception, message, args);
                break;
            case LogLevel.None:
                break;
            default:
                _logger.LogInformation(exception, message, args);
                break;
        }
    }

    public void LogInformation(string category, string message, params object[] args)
    {
        Log(LogLevel.Information, category, message, args);
    }

    public void LogWarning(string category, string message, params object[] args)
    {
        Log(LogLevel.Warning, category, message, args);
    }

    public void LogError(string category, string message, params object[] args)
    {
        Log(LogLevel.Error, category, message, args);
    }

    public void LogError(string category, Exception exception, string message, params object[] args)
    {
        Log(LogLevel.Error, category, exception, message, args);
    }

    public void LogDebug(string category, string message, params object[] args)
    {
        Log(LogLevel.Debug, category, message, args);
    }

    public void LogTrace(string category, string message, params object[] args)
    {
        Log(LogLevel.Trace, category, message, args);
    }

    public void LogCritical(string category, string message, params object[] args)
    {
        Log(LogLevel.Critical, category, message, args);
    }

    public void LogCritical(
        string category,
        Exception exception,
        string message,
        params object[] args
    )
    {
        Log(LogLevel.Critical, category, exception, message, args);
    }

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return _logger.BeginScope(state)!;
    }

    public IDisposable BeginCategoryScope(string category)
    {
        return _logger.BeginScope(new Dictionary<string, object> { ["Category"] = category })!;
    }
}
