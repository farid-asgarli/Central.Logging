using Microsoft.Extensions.Logging;

namespace Central.Logging.Abstractions;

/// <summary>
/// Interface for Extended logging with enhanced categorization
/// </summary>
public interface IBaseLogger
{
    /// <summary>
    /// Logs a message with the specified category and level
    /// </summary>
    void Log(LogLevel level, string category, string message, params object[] args);

    /// <summary>
    /// Logs a message with the specified category, level, and exception
    /// </summary>
    void Log(
        LogLevel level,
        string category,
        Exception exception,
        string message,
        params object[] args
    );

    /// <summary>
    /// Logs an information message with the specified category
    /// </summary>
    void LogInformation(string category, string message, params object[] args);

    /// <summary>
    /// Logs a warning message with the specified category
    /// </summary>
    void LogWarning(string category, string message, params object[] args);

    /// <summary>
    /// Logs an error message with the specified category
    /// </summary>
    void LogError(string category, string message, params object[] args);

    /// <summary>
    /// Logs an error message with exception and the specified category
    /// </summary>
    void LogError(string category, Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs a debug message with the specified category
    /// </summary>
    void LogDebug(string category, string message, params object[] args);

    /// <summary>
    /// Logs a trace message with the specified category
    /// </summary>
    void LogTrace(string category, string message, params object[] args);

    /// <summary>
    /// Logs a critical message with the specified category
    /// </summary>
    void LogCritical(string category, string message, params object[] args);

    /// <summary>
    /// Logs a critical message with exception and the specified category
    /// </summary>
    void LogCritical(string category, Exception exception, string message, params object[] args);

    /// <summary>
    /// Creates a logger scope with additional properties
    /// </summary>
    IDisposable BeginScope<TState>(TState state)
        where TState : notnull;

    /// <summary>
    /// Creates a logger scope with a category
    /// </summary>
    IDisposable BeginCategoryScope(string category);
}
