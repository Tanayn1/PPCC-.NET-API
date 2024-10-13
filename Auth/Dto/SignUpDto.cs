namespace Auth.Dto.SignUpDto;


public class SignUpDto 
{

    public required string Email { get; set; }

    public required string Name { get; set; }
    public required string Password { get; set; }
}