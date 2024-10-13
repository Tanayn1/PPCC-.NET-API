using System.Text.Json.Serialization;

namespace Campaign.Dto.Responses.Extraction.Sitelinks.SitelinkGroup;

public class Sitelink 
{
    [JsonPropertyName("name")]
    public string name { get; set; } = default!;

    [JsonPropertyName("final_url")]
    public string final_url { get; set; } = default!;

    [JsonPropertyName("description_1")]
    public string description_1 { get; set; } = default!;

    [JsonPropertyName("description_2")]
    public string description_2 { get; set; } = default!;


}