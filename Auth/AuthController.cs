namespace Auth.AuthController;

using Auth.Dto.RefreshDto;
using Auth.Dto.Reponses.AuthResponse;
using Auth.Dto.SignInDto;
using Auth.Dto.SignUpDto;
using Auth.Interface.IAuthService;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : Controller 
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) 
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] SignUpDto dto) 
    {
        var res = _authService.SignUp(dto);

        if (res.Success == false) {
            return BadRequest(new 
            {
                error = res.Message
            });
        }
        return Ok(new 
        {
            accessToken = res.AccessToken,
            refreshToken = res.RefreshToken,
            message = res.Message
        });
    }

    [HttpPost("signIn")]
    public IActionResult SignIn([FromBody] SignInDto dto) 
    {
        var res = _authService.SignIn(dto);

        if (res.Success == false) return BadRequest(new 
        {
            error = res.Message
        });

        return Ok(new 
        {
            accessToken = res.AccessToken,
            refreshToken = res.RefreshToken,
            message = res.Message
        });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshDto dto) {
        var res = _authService.RefreshTokens(dto);

        if (res.Success == false) return Unauthorized(new 
        {
            error = res.Message
        });

        return Ok(new 
        {
            accessToken = res.AccessToken
        });
    }
}