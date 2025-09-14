using Central.Logging.Abstractions;

namespace Central.Logging.Elasticsearch.Configuration;

public class CentralLoggingConfigurationWithElastic : CentralLoggingConfiguration
{
    public ElasticsearchConfiguration Elasticsearch { get; set; } = null!;
}
