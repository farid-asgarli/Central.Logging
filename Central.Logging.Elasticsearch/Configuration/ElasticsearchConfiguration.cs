namespace Central.Logging.Elasticsearch.Configuration;

public class ElasticsearchConfiguration
{
    public string Url { get; set; } = GetUrl();
    public ElasticsearchAuthenticationConfiguration Authentication { get; set; } = new();
    public ElasticsearchIndexConfiguration Index { get; set; } = new();
    public ElasticsearchSinkConfiguration Sink { get; set; } = new();

    public string GetIndex() => Index.GetIndexFormat();

    public static string GetUrl()
    {
        bool hasUrl = urlMappings.TryGetValue(WebEnv.CurrentEnv, out string? elasticUrl);
        return hasUrl ? elasticUrl! : urlMappings[WebEnv.Alpha]!;
    }

    private static readonly IDictionary<string, string> urlMappings = new Dictionary<string, string>
    {
        { WebEnv.Alpha, "https://elasti-dev.pasha-life.az/" },
        { WebEnv.Beta, "https://elasti-dev.pasha-life.az/" },
        { WebEnv.Development, "https://elasti-dev.pasha-life.az/" },
        { WebEnv.Production, "https://elasti-prod.pasha-life.az/" },
    };
}

public class ElasticsearchAuthenticationConfiguration
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ApiKey { get; set; }
    public bool HasBasicAuth => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
    public bool HasApiKey => !string.IsNullOrEmpty(ApiKey);
}

public class ElasticsearchIndexConfiguration
{
    private const string _defaultDateFormat = "yyyy.MM.dd";
    public string? Namespace { get; set; }
    public string? ApplicationName { get; set; }
    public string DateFormat { get; set; } = _defaultDateFormat;

    public string GetIndexFormat()
    {
        var environment = WebEnv.CurrentEnv ?? "unknown";
        const string managed = nameof(managed);
        var ns = Namespace?.ToLowerInvariant() ?? "default";
        var appName = GetApplicationName();

        return $"{environment}-{managed}-{ns}-{appName}-{{0:{DateFormat}}}";
    }

    private string GetApplicationName()
    {
        if (!string.IsNullOrEmpty(ApplicationName))
            return SanitizeForIndex(ApplicationName.ToLowerInvariant());

        var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        if (!string.IsNullOrEmpty(assemblyName))
            return SanitizeForIndex(assemblyName.ToLowerInvariant());

        var domainName = AppDomain.CurrentDomain.FriendlyName;
        return SanitizeForIndex(domainName.ToLowerInvariant());
    }

    private static string SanitizeForIndex(string input)
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
}

public class ElasticsearchSinkConfiguration
{
    public bool AutoRegisterTemplate { get; set; } = true;
    public int NumberOfShards { get; set; } = 1;
    public int NumberOfReplicas { get; set; } = 0;
    public TimeSpan? BufferBaseFilename { get; set; }
    public int BatchPostingLimit { get; set; } = 50;
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);
}
