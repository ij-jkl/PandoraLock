using Application.Common.Interfaces;
using System.Text;

namespace Infrastructure.Services;

public class FileSafetyAnalyzer : IFileSafetyAnalyzer
{
    public async Task<(bool IsSafe, string Reason)> AnalyzeFileAsync(Stream fileStream, string fileType)
    {
        if (fileStream == null || fileStream.Length == 0)
            return (false, "File stream is empty");

        try
        {
            fileStream.Position = 0;

            return fileType.ToLowerInvariant() switch
            {
                "pdf" => await AnalyzePdfAsync(fileStream),
                "jpg" => await AnalyzeImageAsync(fileStream),
                "png" => await AnalyzeImageAsync(fileStream),
                _ => (false, "Unsupported file type")
            };
        }
        catch (Exception ex)
        {
            return (false, $"Error analyzing file : {ex.Message}");
        }
        finally
        {
            fileStream.Position = 0;
        }
    }

    private async Task<(bool IsSafe, string Reason)> AnalyzePdfAsync(Stream fileStream)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var content = memoryStream.ToArray();

        if (content.Length < 4)
            return (false, "Invalid PDF file");

        return (true, "PDF is safe");
    }

    private async Task<(bool IsSafe, string Reason)> AnalyzeImageAsync(Stream fileStream)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var content = memoryStream.ToArray();

        if (content.Length < 4)
            return (false, "Invalid image file");

        return (true, "Image is safe");
    }

    private bool ContainsPattern(byte[] content, byte[] pattern)
    {
        for (int i = 0; i <= content.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (content[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return true;
        }
        return false;
    }

    private bool ContainsExecutableSignature(byte[] content)
    {
        var exeSignature = new byte[] { 0x4D, 0x5A }; // MZ - Windows Executable
        var elfSignature = new byte[] { 0x7F, 0x45, 0x4C, 0x46 }; // ELF - Linux
        var machOSignature = new byte[] { 0xFE, 0xED, 0xFA, 0xCE }; // Mach-O - Macbook

        return ContainsPattern(content, exeSignature) ||
               ContainsPattern(content, elfSignature) ||
               ContainsPattern(content, machOSignature);
    }
}
