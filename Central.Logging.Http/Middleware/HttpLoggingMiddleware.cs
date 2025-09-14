using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Central.Logging.Abstractions;
using Central.Logging.Http.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Central.Logging.Http.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IBaseLogger _logger;
    private readonly HttpLoggingMiddlewareConfiguration _config;

    // Pre-compiled regexes for better performance
    private readonly Regex[] _excludedPathRegexes;
    private readonly Dictionary<string, Regex> _sensitiveFieldRegexes;
    private readonly Func<string> _requestIdGenerator;

    public HttpLoggingMiddleware(
        RequestDelegate next,
        IBaseLogger logger,
        IOptions<HttpLoggingMiddlewareConfiguration> config
    )
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));

        ValidateConfiguration(_config);

        // Pre-compile regexes for excluded paths
        _excludedPathRegexes =
        [
            .. _config.ExcludedPaths.Select(path => new Regex(
                $"^{path.Replace("*", ".*").ToLowerInvariant()}$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            )),
        ];

        // Pre-compile regexes for sensitive field masking
        _sensitiveFieldRegexes = [];
        foreach (var field in _config.SensitiveFields)
        {
            var jsonPattern = $@"""({field})""\s*:\s*""([^""]+)""";
            var formPattern = $@"({field})=([^&\s]+)";
            _sensitiveFieldRegexes[$"{field}_json"] = new Regex(
                jsonPattern,
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );
            _sensitiveFieldRegexes[$"{field}_form"] = new Regex(
                formPattern,
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );
        }

        // Configure request ID generator
        _requestIdGenerator =
            _config.RequestIdGenerator ?? (() => Guid.NewGuid().ToString("N")[..8]);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipLogging(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = _requestIdGenerator();

        using var scope = _logger.BeginScope(
            new Dictionary<string, object> { ["RequestId"] = requestId }
        );

        string? requestBody = null;
        string? responseBody = null;
        Stream? originalResponseBody = null;
        ResponseCapturingStream? responseCapturingStream = null;

        try
        {
            // Capture request
            if (_config.LogRequests)
            {
                requestBody = await CaptureRequestBodyAsync(context.Request);
                LogRequest(context, requestId, requestBody);
            }

            // Setup response capture
            if (_config.LogResponses)
            {
                originalResponseBody = context.Response.Body;
                responseCapturingStream = new(originalResponseBody, _config.MaxResponseBodySize);
                context.Response.Body = responseCapturingStream;
            }

            // Execute next middleware
            await _next(context);

            stopwatch.Stop();

            // Capture and log response
            if (responseCapturingStream != null)
            {
                responseBody = responseCapturingStream.GetCapturedContent();
                LogResponse(context, requestId, responseBody, stopwatch.ElapsedMilliseconds);
            }

            LogPerformance(context, requestId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(context, requestId, ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            if (originalResponseBody is not null)
                context.Response.Body = originalResponseBody;

            responseCapturingStream?.Dispose();
        }
    }

    private static void ValidateConfiguration(HttpLoggingMiddlewareConfiguration config)
    {
        if (config.MaxRequestBodySize <= 0)
            throw new ArgumentException("MaxRequestBodySize must be positive", nameof(config));

        if (config.MaxResponseBodySize <= 0)
            throw new ArgumentException("MaxResponseBodySize must be positive", nameof(config));

        if (config.ExcludedPaths is null)
            throw new ArgumentException("ExcludedPaths cannot be null", nameof(config));

        if (config.SensitiveFields is null)
            throw new ArgumentException("SensitiveFields cannot be null", nameof(config));

        if (config.ExcludedHeaders is null)
            throw new ArgumentException("ExcludedHeaders cannot be null", nameof(config));

        if (config.LoggableContentTypes is null)
            throw new ArgumentException("LoggableContentTypes cannot be null", nameof(config));

        // Validate reasonable size limits to prevent memory issues
        const int maxReasonableSize = 50 * 1024 * 1024; // 50MB
        if (config.MaxRequestBodySize > maxReasonableSize)
            throw new ArgumentException(
                $"MaxRequestBodySize cannot exceed {maxReasonableSize} bytes",
                nameof(config)
            );

        if (config.MaxResponseBodySize > maxReasonableSize)
            throw new ArgumentException(
                $"MaxResponseBodySize cannot exceed {maxReasonableSize} bytes",
                nameof(config)
            );
    }

    private bool ShouldSkipLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        return _excludedPathRegexes.Any(regex => regex.IsMatch(path));
    }

    private void LogRequest(HttpContext context, string requestId, string? requestBody)
    {
        var request = context.Request;

        _logger.LogInformation(
            "HttpRequest",
            "Request {RequestId}: {Method} {Path} from {RemoteIP} | ContentType: {ContentType} | UserAgent: {UserAgent}",
            requestId,
            request.Method,
            request.Path,
            GetClientIpAddress(context),
            request.ContentType ?? "none",
            GetUserAgent(request)
        );

        if (_config.LogRequestHeaders)
        {
            var headers = GetFilteredHeaders(request.Headers);
            _logger.LogDebug(
                "HttpRequest",
                "Request {RequestId} Headers: {@Headers}",
                requestId,
                headers
            );
        }

        if (_config.LogRequestBody && !string.IsNullOrEmpty(requestBody))
        {
            var maskedBody = _config.MaskSensitiveData
                ? MaskSensitiveData(requestBody)
                : requestBody;
            _logger.LogInformation(
                "HttpRequest",
                "Request {RequestId} Body: {Body}",
                requestId,
                maskedBody
            );
        }
    }

    private void LogResponse(
        HttpContext context,
        string requestId,
        string? responseBody,
        long elapsedMs
    )
    {
        var response = context.Response;
        var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(
            logLevel,
            "HttpResponse",
            "Response {RequestId}: {StatusCode} | ContentType: {ContentType} | Duration: {ElapsedMs}ms",
            requestId,
            response.StatusCode,
            response.ContentType ?? "none",
            elapsedMs
        );

        if (_config.LogResponseHeaders)
        {
            var headers = GetFilteredHeaders(response.Headers);
            _logger.LogDebug(
                "HttpResponse",
                "Response {RequestId} Headers: {@Headers}",
                requestId,
                headers
            );
        }

        if (_config.LogRequestBody && !string.IsNullOrEmpty(responseBody))
        {
            var maskedBody = _config.MaskSensitiveData
                ? MaskSensitiveData(responseBody)
                : responseBody;
            _logger.LogInformation(
                "HttpResponse",
                "Response {RequestId} Body: {Body}",
                requestId,
                maskedBody
            );
        }
    }

    private void LogError(
        HttpContext context,
        string requestId,
        Exception exception,
        long elapsedMs
    )
    {
        _logger.LogError(
            "HttpError",
            exception,
            "Error {RequestId}: {Method} {Path} failed after {ElapsedMs}ms | Exception: {ExceptionType}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            elapsedMs,
            exception.GetType().Name
        );
    }

    private void LogPerformance(HttpContext context, string requestId, long elapsedMs)
    {
        if (!_config.LogPerformanceMetrics)
            return;

        var logLevel =
            elapsedMs > 5000 ? LogLevel.Warning
            : elapsedMs > 1000 ? LogLevel.Information
            : LogLevel.Debug;

        _logger.Log(
            logLevel,
            "Performance",
            "Performance {RequestId}: {Method} {Path} completed in {ElapsedMs}ms",
            requestId,
            context.Request.Method,
            context.Request.Path,
            elapsedMs
        );
    }

    private async Task<string?> CaptureRequestBodyAsync(HttpRequest request)
    {
        // Check content type first for efficiency
        if (
            string.IsNullOrEmpty(request.ContentType)
            || !_config.LoggableContentTypes.Any(type =>
                request.ContentType.StartsWith(type, StringComparison.OrdinalIgnoreCase)
            )
        )
            return null;

        try
        {
            // Enable buffering to allow multiple reads
            request.EnableBuffering();

            // Handle missing Content-Length gracefully (common with chunked transfers)
            var declaredContentLength = request.ContentLength;
            var maxBytesToRead = declaredContentLength.HasValue
                ? Math.Min((int)declaredContentLength.Value, _config.MaxRequestBodySize)
                : _config.MaxRequestBodySize;

            // Check if declared content length exceeds our limit
            if (declaredContentLength > _config.MaxRequestBodySize)
            {
                _logger.LogDebug(
                    "HttpRequest",
                    "Skipping request body capture - declared content length {ContentLength} exceeds max size {MaxSize}",
                    declaredContentLength,
                    _config.MaxRequestBodySize
                );
                return null;
            }

            // Store current position and reset to beginning
            var originalPosition = request.Body.Position;
            request.Body.Position = 0;

            // Read body content with proper buffer management
            using MemoryStream memoryStream = new();
            var buffer = new byte[4096];
            var totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await request.Body.ReadAsync(buffer)) > 0)
            {
                if (totalBytesRead + bytesRead > maxBytesToRead)
                {
                    var remainingBytes = maxBytesToRead - totalBytesRead;
                    if (remainingBytes > 0)
                    {
                        memoryStream.Write(buffer, 0, remainingBytes);
                        totalBytesRead += remainingBytes;
                    }
                    break;
                }

                memoryStream.Write(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
            }

            request.Body.Position = originalPosition;

            if (totalBytesRead == 0)
                return null;

            var bodyBytes = memoryStream.ToArray();
            return Encoding.UTF8.GetString(bodyBytes);
        }
        catch (IOException ioEx)
        {
            _logger.LogWarning(
                nameof(HttpRequest),
                "Failed to read request body due to IO error: {Error}",
                ioEx.Message
            );
            return null;
        }
        catch (InvalidOperationException opEx)
        {
            _logger.LogWarning(
                nameof(HttpRequest),
                "Failed to read request body due to invalid operation: {Error}",
                opEx.Message
            );
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "HttpRequest",
                ex,
                "Unexpected error while reading request body: {Error}",
                ex.Message
            );
            return null;
        }
    }

    private Dictionary<string, string> GetFilteredHeaders(IHeaderDictionary headers) =>
        headers
            .Where(h => !_config.ExcludedHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

    private static string GetUserAgent(HttpRequest request) =>
        request.Headers.TryGetValue("User-Agent", out var userAgent)
            ? userAgent.ToString()
            : "Unknown";

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP first
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var ip = realIp.ToString();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string MaskSensitiveData(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        var result = content;
        foreach (var field in _config.SensitiveFields)
        {
            // Use pre-compiled regexes for better performance
            if (_sensitiveFieldRegexes.TryGetValue($"{field}_json", out var jsonRegex))
                result = jsonRegex.Replace(result, $@"""{field}"":""***MASKED***""");

            if (_sensitiveFieldRegexes.TryGetValue($"{field}_form", out var formRegex))
                result = formRegex.Replace(result, $"{field}=***MASKED***");
        }
        return result;
    }
}
