namespace BusinessLayer.Dto.Logging;

public class RequestLogDto
{
    public string Method { get; set; } = "";
    public string Path { get; set; } = "";
    public string Query { get; set; } = "";
    public string User { get; set; } = "";
    public string Source { get; set; } = "";
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
}