using Campaign.Dto.CampaignDto;
using Campaign.Dto.MockUpDto;
using Campaign.Dto.Responses.CampaignGenerateResponse;
using Campaign.Dto.Responses.GenerateMockUpResponse;

namespace Campaign.Interface.ICampaignService;

public interface ICampaignService 
{
    public Task<GenerateMockUpResponse> GenerateMockup(MockUpDto dto, IConfiguration configuration);

    public Task<CampaignGenerateResponse> CreateCampaign(CampaignDto dto, IConfiguration configuration);
}