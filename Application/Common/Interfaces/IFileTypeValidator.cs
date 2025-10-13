namespace Application.Common.Interfaces;

public interface IFileTypeValidator
{
    bool IsValidFileType(Stream fileStream, out string detectedType);
    bool IsAllowedFileType(string fileType);
}
