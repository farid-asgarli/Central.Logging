namespace Central.Logging.Abstractions.Utilities;

/// <summary>
/// Utilities for working with index names and application identification
/// </summary>
public static class IndexUtilities
{
    /// <summary>
    /// Sanitizes a string to be safe for use in index names, file names, or identifiers
    /// </summary>
    /// <param name="input">The input string to sanitize</param>
    /// <returns>A sanitized string safe for various naming purposes</returns>
    public static string SanitizeForIndex(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "app";

        var sanitized = input
            .Replace(".exe", "")
            .Replace(".dll", "")
            .Replace("_", "-")
            .Replace(" ", "-");

        if (sanitized.StartsWith("-") || sanitized.StartsWith("."))
            sanitized = "app" + sanitized;

        return string.IsNullOrEmpty(sanitized) ? "app" : sanitized;
    }

    /// <summary>
    /// Gets the application name from various sources, falling back to sensible defaults
    /// </summary>
    /// <param name="providedApplicationName">Optional explicitly provided application name</param>
    /// <returns>A sanitized application name suitable for index names</returns>
    public static string GetApplicationName(string? providedApplicationName = null)
    {
        if (!string.IsNullOrEmpty(providedApplicationName))
            return SanitizeForIndex(providedApplicationName.ToLowerInvariant());

        var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        if (!string.IsNullOrEmpty(assemblyName))
            return SanitizeForIndex(assemblyName.ToLowerInvariant());

        var domainName = AppDomain.CurrentDomain.FriendlyName;
        return SanitizeForIndex(domainName.ToLowerInvariant());
    }

    /// <summary>
    /// Gets the application name from a CentralLoggingConfiguration, with fallbacks
    /// </summary>
    /// <param name="config">The logging configuration</param>
    /// <returns>A sanitized application name</returns>
    public static string GetApplicationNameFromConfig(CentralLoggingConfiguration config)
    {
        return GetApplicationName(config.ApplicationName);
    }

    /// <summary>
    /// Gets the namespace from a configuration or returns default
    /// </summary>
    /// <param name="providedNamespace">Optional explicitly provided namespace</param>
    /// <returns>A sanitized namespace</returns>
    public static string GetNamespace(string? providedNamespace = null)
    {
        return string.IsNullOrEmpty(providedNamespace)
            ? "default"
            : providedNamespace.ToLowerInvariant();
    }

    /// <summary>
    /// Gets the environment from a configuration with fallback to environment variables
    /// </summary>
    /// <param name="providedEnvironment">Optional explicitly provided environment</param>
    /// <returns>A sanitized environment name</returns>
    public static string GetEnvironment(string? providedEnvironment = null)
    {
        if (!string.IsNullOrEmpty(providedEnvironment))
            return providedEnvironment.ToLowerInvariant();

        var envVar = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return !string.IsNullOrEmpty(envVar) ? envVar.ToLowerInvariant() : "unknown";
    }
}
