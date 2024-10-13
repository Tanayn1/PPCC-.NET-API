using System.Text.Json.Serialization;
using Campaign.Dto.Responses.MockUp.Sitelinks;

namespace Campaign.Dto.Responses.MockUp;

    public class MockUpResponse 
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = default!;

        [JsonPropertyName("display_url")]
        public string DisplayUrl { get; set; } = default!;

        [JsonPropertyName("headline")]
        public string Headline { get; set; } = default!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;

        [JsonPropertyName("sitelinks")]
        public SiteLink[] Sitelinks { get; set; } = default!;
    }

  

