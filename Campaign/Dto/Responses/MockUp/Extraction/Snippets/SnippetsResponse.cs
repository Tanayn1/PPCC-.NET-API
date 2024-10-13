
using System.Text.Json.Serialization;
using Campaign.Dto.Responses.Extraction.Snippets.Snippet;

namespace Campaign.Dto.Responses.Extraction.Snippets.SnippetsResponse;

public class SnippetsResponse 
{
    [JsonPropertyName("snippet_group_1")]
    public SnippetGroup snippet_group_1  { get; set; } = default!; 

    [JsonPropertyName("snippet_group_2")]
    public SnippetGroup snippet_group_2  { get; set; } = default!; 

}