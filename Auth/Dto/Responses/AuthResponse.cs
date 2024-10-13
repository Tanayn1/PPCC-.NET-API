namespace Auth.Dto.Reponses.AuthResponse;


public class AuthResponse 
{
    public bool Success;

    public required string Message;

    public string? AccessToken;

    public string? RefreshToken;

}