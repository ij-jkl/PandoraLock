namespace Application.Common.Models;

public class ResponseObjectJsonDto<T>
{
    public string Message { get; set; } = default!;
    public int Code { get; set; }
    public T? Response { get; set; }
    public List<string>? ValidationErrors { get; set; }
}
