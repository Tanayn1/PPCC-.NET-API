using System.Text.Json.Serialization;

namespace Campaign.Dto.Responses.Extraction.BrandNameResponse;

public class BrandNameResponse
{
    [JsonPropertyName("brandName")]
    public string brandName { get; set; } = default!;
}