namespace Auth.AuthService;

using Auth.Dto.RefreshDto;
using Auth.Dto.Reponses.AuthResponse;
using Auth.Dto.SignInDto;
using Auth.Dto.SignUpDto;
using Auth.Interface.IAuthService;
using Database.Entites.User;
using Database.PpccDbContext;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AuthService : IAuthService
{
    private readonly PpccDbContext DbContext; 
    public AuthService(PpccDbContext dbContext) 
    {
        DbContext = dbContext;
    }

    public AuthResponse SignUp(SignUpDto dto) 
    {
        if (!dto.Email.Contains("@")) {
            return new AuthResponse {
                Success = false,
                Message = "Email is not valid"
            };
        } 

        var hashedPassword = Hash(dto.Password);

        var newUser = new User 
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            HashedPassword = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Add<User>(newUser);

        var ( accessToken, refreshToken ) = SignToken(newUser);
        return new AuthResponse {
            Success = true,
            Message = "Success",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }

    public AuthResponse SignIn(SignInDto dto) 
    {
        var user = DbContext.GetUserByEmail(dto.Email);

        if (user == null) return new AuthResponse {
            Success = false,
            Message = "User with given email does not exist"
        };

        var hashedPassword = Hash(dto.Password);

        if (hashedPassword != user.HashedPassword) return new AuthResponse {
            Success = false,
            Message = "Password Is Incorrect",
        };

        var ( accessToken, refreshToken ) = SignToken(user);

        return new AuthResponse {
            Success = true,
            Message = "Success",
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

    }

    private static ( string accessToken, string refreshToken ) SignToken(User user) 
    {
        var handler = new JwtSecurityTokenHandler();
        var claims = new List<Claim> {
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email)
        };

            var accessToken = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("ApplicationSettings:JwtAccessSecret")!)
                        ),
                    SecurityAlgorithms.HmacSha256Signature)
                );
            var refreshToken = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("ApplicationSettings:JwtRefreshSecret")!)
                        ),
                    SecurityAlgorithms.HmacSha256Signature)
                );
            return (handler.WriteToken(accessToken), handler.WriteToken(refreshToken));
    }

    public AuthResponse RefreshTokens(RefreshDto dto) {
        var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "your-issuer",
                ValidAudience = "your-audience",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("ApplicationSettings:JwtRefreshSecret")!)
                        ), 
                ClockSkew = TimeSpan.Zero // Ensure immediate validation
            };
        try
        {
            var claimsPrincipal = handler.ValidateToken(dto.refreshToken, validationParameters, out SecurityToken validatedToken);

            var email = claimsPrincipal.FindFirst("email");
            Console.WriteLine(email);
            if (email?.ToString() == null ) return new AuthResponse 
            {
                Success = false,
                Message = "Email not in token"
            };

            var user = DbContext.GetUserByEmail(email.ToString());
            if (user == null) return new AuthResponse 
            {
                Success = false,
                Message = "User does not exist"
            };

            var ( accessToken, refreshToken ) = SignToken(user);

            return new AuthResponse 
            {
                Success = true,
                Message = "Success",
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        
        }
        catch (Exception)
        {   
           return new AuthResponse 
           {
            Success = false,
            Message = "",
           };
        }

    }

    private static string Hash(string password ) 
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
        Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        return hashed;
    }

}