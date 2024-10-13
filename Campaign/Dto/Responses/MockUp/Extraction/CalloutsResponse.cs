using System.Text.Json.Serialization;

namespace Campaign.Dto.Responses.Extraction.CalloutsResponse;

public class CalloutsResponse
{
    [JsonPropertyName("callout_1")]
    public string callout_1 { get; set; } = default!;

    [JsonPropertyName("callout_2")]
    public string callout_2 { get; set; } = default!;

    [JsonPropertyName("callout_3")]
    public string callout_3 { get; set; } = default!;

    [JsonPropertyName("callout_4")]
    public string callout_4 { get; set; } = default!;
}