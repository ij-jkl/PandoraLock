using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public FileStorageService(string storagePath)
    {
        _storagePath = storagePath;
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, int userId)
    {
        var userDirectory = Path.Combine(_storagePath, userId.ToString());
        
        if (!Directory.Exists(userDirectory))
        {
            Directory.CreateDirectory(userDirectory);
        }
        
        var filePath = Path.Combine(userDirectory, fileName);
        
        using (var outputStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream);
        }
        
        return filePath;
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

    public bool IsPdfFile(Stream fileStream)
    {
        if (fileStream == null || fileStream.Length == 0)
            return false;

        fileStream.Position = 0;
        var headerBytes = new byte[4];
        var bytesRead = fileStream.Read(headerBytes, 0, 4);
        fileStream.Position = 0;

        if (bytesRead < 4)
            return false;

        return headerBytes[0] == 0x25 && 
               headerBytes[1] == 0x50 && 
               headerBytes[2] == 0x44 && 
               headerBytes[3] == 0x46;
    }
}
