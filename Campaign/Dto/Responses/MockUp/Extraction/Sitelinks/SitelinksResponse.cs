using System.Text.Json.Serialization;
using Campaign.Dto.Responses.Extraction.Sitelinks.SitelinkGroup;

namespace Campaign.Dto.Responses.Extraction.Sitelinks.SitelinksResponse;

public class SitelinksResponse
{
    [JsonPropertyName("sitelink_1")]
    public Sitelink sitelink_1 { get; set; } = default!;

    [JsonPropertyName("sitelink_2")]
    public Sitelink sitelink_2 { get; set; } = default!;

    [JsonPropertyName("sitelink_3")]
    public Sitelink sitelink_3 { get; set; } = default!;

    [JsonPropertyName("sitelink_4")]
    public Sitelink sitelink_4 { get; set; } = default!;
}