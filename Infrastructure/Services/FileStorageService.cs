using Application.Common.Interfaces;
using System.Security.Cryptography;

namespace Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly byte[] _encryptionKey;

    public FileStorageService(string storagePath)
    {
        _storagePath = storagePath;
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }

        var keyString = Environment.GetEnvironmentVariable("FILE_ENCRYPTION_KEY");
        if (string.IsNullOrEmpty(keyString) || keyString.Length != 64)
        {
            throw new InvalidOperationException("FILE_ENCRYPTION_KEY must be set and be 64 hex characters (32 bytes)");
        }

        _encryptionKey = Convert.FromHexString(keyString);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, int userId)
    {
        return await EncryptAndSaveFileAsync(fileStream, fileName, userId);
    }

    public async Task<string> EncryptAndSaveFileAsync(Stream fileStream, string fileName, int userId)
    {
        var userDirectory = Path.Combine(_storagePath, userId.ToString());
        
        if (!Directory.Exists(userDirectory))
        {
            Directory.CreateDirectory(userDirectory);
        }
        
        var filePath = Path.Combine(userDirectory, fileName + ".enc");

        using (var aesGcm = new AesGcm(_encryptionKey, AesGcm.TagByteSizes.MaxSize))
        {
            var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
            RandomNumberGenerator.Fill(nonce);

            using (var inputMemory = new MemoryStream())
            {
                await fileStream.CopyToAsync(inputMemory);
                var plaintext = inputMemory.ToArray();

                var ciphertext = new byte[plaintext.Length];
                var tag = new byte[AesGcm.TagByteSizes.MaxSize];

                aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

                using (var outputStream = new FileStream(filePath, FileMode.Create))
                {
                    await outputStream.WriteAsync(nonce, 0, nonce.Length);
                    await outputStream.WriteAsync(tag, 0, tag.Length);
                    await outputStream.WriteAsync(ciphertext, 0, ciphertext.Length);
                }
            }
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

    public async Task<(Stream fileStream, string contentType, string fileName)?> GetFileAsync(string storagePath)
    {
        if (!File.Exists(storagePath))
        {
            return null;
        }

        using (var aesGcm = new AesGcm(_encryptionKey, AesGcm.TagByteSizes.MaxSize))
        {
            using (var inputStream = new FileStream(storagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
                var tag = new byte[AesGcm.TagByteSizes.MaxSize];

                await inputStream.ReadAsync(nonce, 0, nonce.Length);
                await inputStream.ReadAsync(tag, 0, tag.Length);

                var ciphertextLength = (int)(inputStream.Length - nonce.Length - tag.Length);
                var ciphertext = new byte[ciphertextLength];
                await inputStream.ReadAsync(ciphertext, 0, ciphertextLength);

                var plaintext = new byte[ciphertextLength];

                try
                {
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
                }
                catch (CryptographicException)
                {
                    return null;
                }

                var decryptedStream = new MemoryStream(plaintext);
                decryptedStream.Position = 0;

                var fileName = Path.GetFileNameWithoutExtension(storagePath);
                if (fileName.EndsWith(".enc"))
                {
                    fileName = fileName.Substring(0, fileName.Length - 4);
                }

                var contentType = GetContentType(fileName);

                return (decryptedStream, contentType, fileName);
            }
        }
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    }
}
