using Database.Entites.Generations;
using Database.Entites.User;
using Microsoft.EntityFrameworkCore;

namespace Database.PpccDbContext;

public class PpccDbContext : DbContext {

    public DbSet<User> Users { get; set; }

    public DbSet<Generations> Generations { get; set; }


    public User? GetUserByEmail(string email) 
    {
        return Users.FirstOrDefault(u => u.Email == email);
    }

    public PpccDbContext(DbContextOptions<PpccDbContext> options) : base(options) {}
}