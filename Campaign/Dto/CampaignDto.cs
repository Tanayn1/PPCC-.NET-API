namespace Campaign.Dto.CampaignDto;

public class CampaignDto 
{
    public required string Url { get; set; }

    public bool Headlines { get; set; } 

    public bool Snippets { get; set; }

    public bool Callouts { get; set; }

    public bool Sitelinks { get; set; }

    public int Count { get; set; }
}