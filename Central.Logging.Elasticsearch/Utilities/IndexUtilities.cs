using BaseIndexUtilities = Central.Logging.Abstractions.Utilities.IndexUtilities;

namespace Central.Logging.Elasticsearch.Utilities;

/// <summary>
/// Elasticsearch-specific utilities for working with index names and formatting
/// </summary>
public static class IndexUtilities
{
    private const string DefaultDateFormat = "yyyy.MM.dd";

    /// <summary>
    /// Sanitizes a string to be safe for use in Elasticsearch index names
    /// </summary>
    /// <param name="input">The input string to sanitize</param>
    /// <returns>A sanitized string safe for index names</returns>
    public static string SanitizeForIndex(string input) => BaseIndexUtilities.SanitizeForIndex(input);

    /// <summary>
    /// Gets the application name from various sources, falling back to sensible defaults
    /// </summary>
    /// <param name="providedApplicationName">Optional explicitly provided application name</param>
    /// <returns>A sanitized application name suitable for index names</returns>
    public static string GetApplicationName(string? providedApplicationName = null) => BaseIndexUtilities.GetApplicationName(providedApplicationName);

    /// <summary>
    /// Generates a formatted index pattern for Elasticsearch
    /// </summary>
    /// <param name="environment">The environment (dev, prod, etc.)</param>
    /// <param name="namespace">The namespace or tenant identifier</param>
    /// <param name="applicationName">The application name (optional, will be auto-detected if null)</param>
    /// <param name="dateFormat">The date format pattern (defaults to yyyy.MM.dd)</param>
    /// <returns>A formatted index pattern string</returns>
    public static string GetFormattedIndexName(
        string environment,
        string? @namespace = null,
        string? applicationName = null,
        string? dateFormat = null)
    {
        var env = BaseIndexUtilities.GetEnvironment(environment);
        const string managed = nameof(managed);
        var ns = BaseIndexUtilities.GetNamespace(@namespace);
        var appName = BaseIndexUtilities.GetApplicationName(applicationName);
        var format = dateFormat ?? DefaultDateFormat;

        return $"{env}-{managed}-{ns}-{appName}-{{0:{format}}}";
    }

    /// <summary>
    /// Generates a simple index name without date formatting
    /// </summary>
    /// <param name="environment">The environment (dev, prod, etc.)</param>
    /// <param name="namespace">The namespace or tenant identifier</param>
    /// <param name="applicationName">The application name (optional, will be auto-detected if null)</param>
    /// <returns>A simple index name string</returns>
    public static string GetSimpleIndexName(
        string environment,
        string? @namespace = null,
        string? applicationName = null)
    {
        var env = BaseIndexUtilities.GetEnvironment(environment);
        const string managed = nameof(managed);
        var ns = BaseIndexUtilities.GetNamespace(@namespace);
        var appName = BaseIndexUtilities.GetApplicationName(applicationName);

        return $"{env}-{managed}-{ns}-{appName}";
    }
}