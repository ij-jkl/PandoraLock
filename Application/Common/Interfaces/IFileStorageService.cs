namespace Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName);
    Task DeleteFileAsync(string filePath);
    bool IsPdfFile(Stream fileStream);
}
