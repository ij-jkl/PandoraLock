using Domain.Enums;

namespace Domain.Entities;

public class UserEntity
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.User;
    
    public int FailedLoginAttempts { get; set; } = 0;
    public bool IsLocked { get; set; } = false;
    
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
    public DateTime? LastLoginAt { get; set; }
    
    public ICollection<FileEntity> Files { get; set; } = new List<FileEntity>();
}
