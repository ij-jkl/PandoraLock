namespace Domain.Entities;

public class SharedFileAccessEntity
{
    private static readonly TimeZoneInfo ArgentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

    public int Id { get; set; }
    public int FileId { get; set; }
    public FileEntity File { get; set; } = default!;
    public int SharedWithUserId { get; set; }
    public UserEntity SharedWithUser { get; set; } = default!;
    public DateTime SharedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ArgentinaTimeZone);
    public DateTime? ExpiresAt { get; set; }
    public int DownloadCount { get; set; } = 0;
}
