using Nest;

namespace DataAccessLayer.Models.Logging;

[ElasticsearchType(RelationName = "request_log")]
public class RequestLog
{
    [Date(Name = "@timestamp")]
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    [Keyword(Name = "method")]
    public string Method { get; set; } = "";

    [Keyword(Name = "path")]
    public string Path { get; set; } = "";

    [Text(Name = "query")]
    public string Query { get; set; } = "";

    [Keyword(Name = "user")]
    public string User { get; set; } = "";

    [Keyword(Name = "source")]
    public string Source { get; set; } = "";

    [Number(Name = "status_code")]
    public int StatusCode { get; set; }

    [Number(Name = "duration_ms")]
    public long DurationMs { get; set; }
}