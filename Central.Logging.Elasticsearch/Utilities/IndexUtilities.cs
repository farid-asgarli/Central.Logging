using BaseIndexUtilities = Central.Logging.Abstractions.Utilities.IndexUtilities;

namespace Central.Logging.Elasticsearch.Utilities;

/// <summary>
/// Elasticsearch-specific utilities for working with index names and formatting
/// </summary>
public static class IndexUtilities
{
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
        string dateFormat,
        string? @namespace = null,
        string? applicationName = null
    )
    {
        var env = BaseIndexUtilities.GetEnvironment(environment);
        const string managed = nameof(managed);
        var ns = BaseIndexUtilities.GetNamespace(@namespace);
        var appName = BaseIndexUtilities.GetApplicationName(applicationName);

        return $"{env}-{managed}-{ns}-{appName}-{{0:{dateFormat}}}";
    }
}
