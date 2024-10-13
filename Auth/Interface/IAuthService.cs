using Auth.Dto.RefreshDto;
using Auth.Dto.Reponses.AuthResponse;
using Auth.Dto.SignInDto;
using Auth.Dto.SignUpDto;

namespace Auth.Interface.IAuthService;

public interface IAuthService
{
    public AuthResponse SignUp(SignUpDto dto);

    public AuthResponse SignIn(SignInDto dto);

    public AuthResponse RefreshTokens(RefreshDto dto);

}