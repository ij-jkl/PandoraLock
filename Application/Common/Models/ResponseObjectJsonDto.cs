using System.Net;

namespace Application.Common.Models;

public class ResponseObjectJsonDto
{
    public string Message { get; set; } = default!;
    public int Code { get; set; }
    public object? Response { get; set; }
    // When we use dictionary here its cleaner and easier to read the validation errors.
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
