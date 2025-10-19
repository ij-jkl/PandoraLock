namespace Application.DTOs;

public class FileDownloadDto
{
    public Stream FileStream { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
}
