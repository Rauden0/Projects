using DataAccessLayer.Models.Logging;
using LanguageExt.Common;
using Nest;


namespace BusinessLayer.Service.Logging;

public class AuditLogService : IAuditLogService
{
    private readonly IElasticClient _client;
    private const string Index = "audits-log";

    public AuditLogService(IElasticClient elasticClient)
    {
        _client = elasticClient;
    }

    public async Task LogAsync(string user, string entity, string entityId, string action, string result)
    {
        var countResponse = await _client.CountAsync<AuditLog>(c => c
            .Index(Index)
            .Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.Term(t => t.Entity.Suffix("keyword"), entity),
                        m => m.Term(t => t.EntityId.Suffix("keyword"), entityId),
                        m => m.Term(t => t.Action.Suffix("keyword"), action)
                    )
                )
            )
        );

        var newLog = new AuditLog
        {
            User = user,
            Entity = entity,
            EntityId = entityId,
            Action = action,
            Result = result,
            EditCount = (int)countResponse.Count + 1
        };

        await _client.IndexAsync(newLog, i => i.Index(Index));
    }

    public async Task<Result<List<AuditLog>>> GetLogsAsync(string? entityId = null, string? entity = null)
    {
        var musts = new List<Func<QueryContainerDescriptor<AuditLog>, QueryContainer>>();

        if (!string.IsNullOrEmpty(entity))
            musts.Add(m => m.Term(t => t.Entity.Suffix("keyword"), entity));

        if (!string.IsNullOrEmpty(entityId))
            musts.Add(m => m.Term(t => t.EntityId.Suffix("keyword"), entityId));

        var response = await _client.SearchAsync<AuditLog>(s => s
            .Index(Index)
            .Query(q => musts.Count == 0
                ? q.MatchAll()
                : q.Bool(b => b.Must(musts.ToArray())))
            .Sort(sd => sd.Descending(f => f.TimestampUtc))
        );

        return response.Documents.ToList();
    }
}