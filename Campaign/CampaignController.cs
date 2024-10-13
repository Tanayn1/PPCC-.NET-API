using Campaign.Dto.CampaignDto;
using Campaign.Dto.MockUpDto;
using Campaign.Interface.ICampaignService;
using Database.PpccDbContext;
using Microsoft.AspNetCore.Mvc;

namespace Campaign.CampaignController;

[Route("api/v1/campaign")]
[ApiController]
public class CampaignController : Controller 
{

    private readonly ICampaignService _campaignService;
    private readonly IConfiguration _configuration;

    public CampaignController(ICampaignService campaignService, IConfiguration configuration) {
        _campaignService = campaignService;
        _configuration = configuration;

    }


    [HttpPost("mockup")]
    public async Task<IActionResult> GenerateMockup(MockUpDto dto) 
    {
        var res = await _campaignService.GenerateMockup(dto, _configuration);

        if (res.Success == false) return StatusCode(500, new 
        {
            error = res.Message
        });
        return Ok(new 
        {
            mockup = res.Mockup,
            message = res.Message
        });
    }

    [HttpPost("campaign")]
    public async Task<IActionResult> CreateCampaign(CampaignDto dto) 
    {
        var res = await _campaignService.CreateCampaign(dto, _configuration);

        if (res.Success == false) {
            return StatusCode(500, new 
            {
                error = res.Message
            });
        }
        return Ok();
    }

}
