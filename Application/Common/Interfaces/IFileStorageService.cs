namespace Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, int userId);
    Task DeleteFileAsync(string filePath);
    Task<(Stream fileStream, string contentType, string fileName)?> GetFileAsync(string storagePath);
    Task<string> EncryptAndSaveFileAsync(Stream fileStream, string fileName, int userId);
}
