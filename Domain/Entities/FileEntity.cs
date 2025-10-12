namespace Domain.Entities;

public class FileEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string StoragePath { get; set; } = default!;
    public long SizeInBytes { get; set; }
    public string ContentType { get; set; } = default!;
    public int UserId { get; set; }
    public UserEntity User { get; set; } = default!;
    public DateTime UploadedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
    public DateTime? UpdatedAt { get; set; }
}
