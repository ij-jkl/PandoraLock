namespace Application.DTOs;

public class CachedFileDto
{
    public byte[] FileContent { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
}
