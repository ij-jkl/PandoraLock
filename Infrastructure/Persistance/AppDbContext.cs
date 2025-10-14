using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<UserEntity> Users { get; set; } = default!;
    public DbSet<FileEntity> Files { get; set; } = default!;
    public DbSet<SharedFileAccessEntity> SharedFileAccess { get; set; } = default!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileEntity>()
            .HasOne(f => f.User)
            .WithMany(u => u.Files)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SharedFileAccessEntity>()
            .HasOne(s => s.File)
            .WithMany(f => f.SharedAccess)
            .HasForeignKey(s => s.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SharedFileAccessEntity>()
            .HasOne(s => s.SharedWithUser)
            .WithMany(u => u.SharedFilesAccess)
            .HasForeignKey(s => s.SharedWithUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SharedFileAccessEntity>()
            .HasIndex(s => new { s.FileId, s.SharedWithUserId })
            .IsUnique();
    }
}

// To create the initial migration and update the database, run the following commands in the terminal, after we are going to run everything just by docker compose:
// dotnet ef migrations add InitialMigration
// dotnet ef database update

