namespace Application.DTOs;

public class FileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public long SizeInBytes { get; set; }
    public string ContentType { get; set; } = default!;
    public bool IsPublic { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SharedFileAccessDto
{
    public int Id { get; set; }
    public int FileId { get; set; }
    public string SharedWithUserEmail { get; set; } = default!;
    public DateTime SharedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int DownloadCount { get; set; }
    public int? MaxDownloads { get; set; }
}
