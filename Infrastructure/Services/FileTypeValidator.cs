using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class FileTypeValidator : IFileTypeValidator
{
    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        {
            "pdf", new[]
            {
                new byte[] { 0x25, 0x50, 0x44, 0x46 }
            }
        },
        {
            "jpg", new[]
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }
            }
        },
        {
            "png", new[]
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        }
    };

    private static readonly string[] AllowedFileTypes = { "pdf", "jpg", "png" };

    public bool IsValidFileType(Stream fileStream, out string detectedType)
    {
        detectedType = string.Empty;

        if (fileStream == null || fileStream.Length == 0)
            return false;

        try
        {
            fileStream.Position = 0;

            var headerBytes = new byte[8];
            var bytesRead = fileStream.Read(headerBytes, 0, 8);

            fileStream.Position = 0;

            if (bytesRead < 4)
                return false;

            foreach (var fileType in FileSignatures)
            {
                foreach (var signature in fileType.Value)
                {
                    if (bytesRead >= signature.Length && HeaderMatches(headerBytes, signature))
                    {
                        detectedType = fileType.Key;
                        return true;
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool IsAllowedFileType(string fileType)
    {
        return AllowedFileTypes.Contains(fileType?.ToLowerInvariant());
    }

    private bool HeaderMatches(byte[] fileHeader, byte[] signature)
    {
        for (int i = 0; i < signature.Length; i++)
        {
            if (fileHeader[i] != signature[i])
                return false;
        }
        return true;
    }
}
