using Nest;

namespace DataAccessLayer.Models.Logging;

[ElasticsearchType(RelationName = "audit_log")]
public class AuditLog
{
    [Date(Name = "@timestamp")]
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    [Keyword(Name = "user")]
    public string User { get; set; } = "";

    [Keyword(Name = "entity")]
    public string Entity { get; set; } = "";

    [Keyword(Name = "entity_id")]
    public string EntityId { get; set; } = "";

    [Keyword(Name = "action")]
    public string Action { get; set; } = "";

    [Keyword(Name = "result")]
    public string Result { get; set; } = "";
    
    [Number(Name = "edit_count")]
    public int EditCount { get; set; }
}