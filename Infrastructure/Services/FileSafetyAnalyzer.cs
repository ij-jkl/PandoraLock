using Application.Common.Interfaces;
using System.Text;

namespace Infrastructure.Services;

public class FileSafetyAnalyzer : IFileSafetyAnalyzer
{
    private static readonly byte[][] PdfJavaScriptPatterns = new[]
    {
        // Searching for the following patterns inside the bytes of the file.
        Encoding.ASCII.GetBytes("/JavaScript"),
        Encoding.ASCII.GetBytes("/JS"),
        Encoding.ASCII.GetBytes("/OpenAction"),
        Encoding.ASCII.GetBytes("/AA"),
        Encoding.ASCII.GetBytes("/Launch"),
        Encoding.ASCII.GetBytes("/SubmitForm"),
        Encoding.ASCII.GetBytes("/ImportData"),
        Encoding.ASCII.GetBytes("/GoTo"),
        Encoding.ASCII.GetBytes("/GoToR"),
        Encoding.ASCII.GetBytes("/GoToE"),
        Encoding.ASCII.GetBytes("/URI"),
        Encoding.ASCII.GetBytes("/EmbeddedFile"),
        Encoding.ASCII.GetBytes("/RichMedia"),
        Encoding.ASCII.GetBytes("/Flash"),
        Encoding.ASCII.GetBytes("/XFA")
    };

    private static readonly byte[][] ImageEmbeddedPatterns = new[]
    {
        Encoding.ASCII.GetBytes("<?php"),
        Encoding.ASCII.GetBytes("<%"),
        Encoding.ASCII.GetBytes("<script"),
        Encoding.ASCII.GetBytes("eval("),
        Encoding.ASCII.GetBytes("base64_decode"),
        Encoding.ASCII.GetBytes("exec("),
        Encoding.ASCII.GetBytes("system("),
        Encoding.ASCII.GetBytes("passthru("),
        Encoding.ASCII.GetBytes("shell_exec")
    };

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

        foreach (var pattern in PdfJavaScriptPatterns)
        {
            if (ContainsPattern(content, pattern))
            {
                var patternName = Encoding.ASCII.GetString(pattern);
                return (false, $"PDF contains suspicious element : {patternName}");
            }
        }

        if (ContainsExecutableSignature(content))
        {
            return (false, "PDF contains embedded executable content");
        }

        return (true, "PDF is safe");
    }

    private async Task<(bool IsSafe, string Reason)> AnalyzeImageAsync(Stream fileStream)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);

        var content = memoryStream.ToArray();

        foreach (var pattern in ImageEmbeddedPatterns)
        {
            if (ContainsPattern(content, pattern))
            {
                var patternName = Encoding.ASCII.GetString(pattern);
                return (false, $"Image contains suspicious embedded content : {patternName}");
            }
        }

        if (ContainsExecutableSignature(content))
        {
            return (false, "Image contains embedded executable content");
        }

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
