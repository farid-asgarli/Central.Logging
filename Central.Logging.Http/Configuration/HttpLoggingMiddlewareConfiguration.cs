using Central.Logging.Http.Constants;

namespace Central.Logging.Http.Configuration;

/// <summary>
/// Configuration for HttpLoggingMiddleware
/// </summary>
public class HttpLoggingMiddlewareConfiguration
{
    /// <summary>
    /// Whether to log incoming requests
    /// </summary>
    public bool LogRequests { get; set; } = true;

    /// <summary>
    /// Whether to request body
    /// </summary>
    public bool LogRequestBody { get; set; } = true;

    /// <summary>
    /// Whether to log outgoing responses
    /// </summary>
    public bool LogResponses { get; set; } = true;

    /// <summary>
    /// Whether to log response body
    /// </summary>
    public bool LogResponseBody { get; set; } = true;

    /// <summary>
    /// Whether to log request headers
    /// </summary>
    public bool LogRequestHeaders { get; set; } = false;

    /// <summary>
    /// Whether to log response headers
    /// </summary>
    public bool LogResponseHeaders { get; set; } = false;

    /// <summary>
    /// Whether to log performance metrics
    /// </summary>
    public bool LogPerformanceMetrics { get; set; } = true;

    /// <summary>
    /// Whether to mask sensitive data in request/response bodies
    /// </summary>
    public bool MaskSensitiveData { get; set; } = true;

    /// <summary>
    /// Maximum size in bytes for request body logging
    /// </summary>
    public int MaxRequestBodySize { get; set; } = 32768; // 32KB

    /// <summary>
    /// Maximum size in bytes for response body logging
    /// </summary>
    public int MaxResponseBodySize { get; set; } = 32768; // 32KB

    /// <summary>
    /// Paths to exclude from logging (supports wildcards with *)
    /// </summary>
    public List<string> ExcludedPaths { get; set; } =
        [
            HttpPaths.Health,
            HttpPaths.Favicon,
            HttpPaths.Metrics,
            HttpPaths.Swagger,
        ];

    /// <summary>
    /// Headers to exclude from logging
    /// </summary>
    public List<string> ExcludedHeaders { get; set; } =
        [
            HttpHeaders.Cookie,
            HttpHeaders.XAPIKey,
            HttpHeaders.SetCookie,
            HttpHeaders.XAuthToken,
            HttpHeaders.Authorization,
        ];

    /// <summary>
    /// Content types that should have their bodies logged
    /// </summary>
    public List<string> LoggableContentTypes { get; set; } =
        [
            HttpContentTypes.TextXml,
            HttpContentTypes.TextPlain,
            HttpContentTypes.ApplicationXml,
            HttpContentTypes.ApplicationJson,
            HttpContentTypes.ApplicationXWWWFormUrlEncoded,
        ];

    /// <summary>
    /// Field names that contain sensitive data and should be masked
    /// </summary>
    public List<string> SensitiveFields { get; set; } =
        [
            SensitiveFieldNames.Key,
            SensitiveFieldNames.Token,
            SensitiveFieldNames.Secret,
            SensitiveFieldNames.Password,
            SensitiveFieldNames.Authorization,
        ];

    /// <summary>
    /// Custom request ID generator function. If null, uses default GUID-based generator.
    /// </summary>
    public Func<string>? RequestIdGenerator { get; set; }
}
