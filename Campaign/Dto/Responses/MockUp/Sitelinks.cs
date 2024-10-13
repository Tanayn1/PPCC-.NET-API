using System.Text.Json.Serialization;

namespace Campaign.Dto.Responses.MockUp.Sitelinks;

  public class SiteLink
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = default!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;
    }