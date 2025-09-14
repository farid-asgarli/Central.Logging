using System.Net.Mime;

namespace Central.Logging.Http.Contents;

public class DefaultContentTypes
{
    public const string ApplicationJson = MediaTypeNames.Application.Json;
    public const string ApplicationXml = MediaTypeNames.Application.Xml;
    public const string TextXml = MediaTypeNames.Text.Xml;
    public const string TextPlain = MediaTypeNames.Text.Plain;
    public const string ApplicationXWWWFormUrlEncoded = "application/x-www-form-urlencoded";
}
