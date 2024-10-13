using System.Text.Json.Serialization;

namespace Campaign.Dto.Responses.Extraction.Snippets.Snippet;

public class SnippetGroup 
{
    [JsonPropertyName("header")]
    public string header { get; set; } = default!;

    [JsonPropertyName("snippet_1")]
    public string snippet_1 { get; set; } = default!;

    [JsonPropertyName("snippet_2")]
    public string snippet_2 { get; set; } = default!;

    [JsonPropertyName("snippet_3")]
    public string snippet_3 { get; set; } = default!;

    [JsonPropertyName("snippet_4")]
    public string snippet_4 { get; set; } = default!;
 
}
