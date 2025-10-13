namespace Application.Common.Interfaces;

public interface IFileSafetyAnalyzer
{
    Task<(bool IsSafe, string Reason)> AnalyzeFileAsync(Stream fileStream, string fileType);
}
