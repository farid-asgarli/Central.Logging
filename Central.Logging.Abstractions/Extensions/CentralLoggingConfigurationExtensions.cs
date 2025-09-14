using Central.Logging.Abstractions.Utilities;

namespace Central.Logging.Abstractions.Extensions;

/// <summary>
/// Extension methods for CentralLoggingConfiguration
/// </summary>
public static class CentralLoggingConfigurationExtensions
{
    /// <summary>
    /// Gets the effective application name, using fallbacks if not explicitly set
    /// </summary>
    /// <param name="config">The logging configuration</param>
    /// <returns>A sanitized application name</returns>
    public static string GetEffectiveApplicationName(this CentralLoggingConfiguration config)
    {
        return IndexUtilities.GetApplicationName(config.ApplicationName);
    }

    /// <summary>
    /// Gets the effective namespace, using fallbacks if not explicitly set
    /// </summary>
    /// <param name="config">The logging configuration</param>
    /// <returns>A sanitized namespace</returns>
    public static string GetEffectiveNamespace(this CentralLoggingConfiguration config)
    {
        return IndexUtilities.GetNamespace(config.Namespace);
    }

    /// <summary>
    /// Gets the effective environment, using fallbacks if not explicitly set
    /// </summary>
    /// <param name="config">The logging configuration</param>
    /// <returns>A sanitized environment name</returns>
    public static string GetEffectiveEnvironment(this CentralLoggingConfiguration config)
    {
        return IndexUtilities.GetEnvironment(config.Environment);
    }

    /// <summary>
    /// Gets a combined identifier string suitable for indexing or identification purposes
    /// </summary>
    /// <param name="config">The logging configuration</param>
    /// <param name="separator">The separator to use between components (defaults to "-")</param>
    /// <returns>A combined identifier string</returns>
    public static string GetIdentifier(
        this CentralLoggingConfiguration config,
        string separator = "-"
    )
    {
        var env = config.GetEffectiveEnvironment();
        var ns = config.GetEffectiveNamespace();
        var app = config.GetEffectiveApplicationName();

        return $"{env}{separator}{ns}{separator}{app}";
    }
}
