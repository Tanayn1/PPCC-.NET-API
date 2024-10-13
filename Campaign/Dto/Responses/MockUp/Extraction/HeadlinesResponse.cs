using System.Text.Json.Serialization;

namespace Campaign.Dto.Responses.Extraction.HeadlinesResponse;

public class HeadlinesResponse
{
    [JsonPropertyName("Headlines")]
    public string[] Headlines { get; set; } = default!;

}