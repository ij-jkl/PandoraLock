namespace Domain.Entities;

public class SharedFileAccessEntity
{
    public int Id { get; set; }
    public int FileId { get; set; }
    public FileEntity File { get; set; } = default!;
    public int SharedWithUserId { get; set; }
    public UserEntity SharedWithUser { get; set; } = default!;
    public DateTime SharedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
}
