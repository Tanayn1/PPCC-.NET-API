namespace Database.Entites.User;

public class User 
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }
    public required string Email { get; set; }

    public int Credits { get; set; }

    public required string HashedPassword { get; set; }

    public required DateTime CreatedAt { get; set; }
}