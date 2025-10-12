using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<UserEntity> Users { get; set; } = default!;
    public DbSet<FileEntity> Files { get; set; } = default!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}

// To create the initial migration and update the database, run the following commands in the terminal, after we are going to run everything just by docker compose:
// dotnet ef migrations add InitialMigration
// dotnet ef database update

