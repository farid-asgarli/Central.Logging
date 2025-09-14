namespace Central.Logging.Elasticsearch;

public class EnvironmentConstants
{
    public static string CurrentEnv =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLowerInvariant()!;
    public const string Development = "development";
    public const string Alpha = "staging";
    public const string Beta = "preprod";
    public const string Production = "production";
}
