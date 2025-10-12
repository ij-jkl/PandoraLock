namespace Application.DTOs;

public class FileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public long SizeInBytes { get; set; }
    public string ContentType { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
