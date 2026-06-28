using DataAccessLayer.Models.Logging;
using LanguageExt.Common;

namespace BusinessLayer.Service.Logging;

public interface IAuditLogService
{
    Task LogAsync(string user, string entity, string entityId, string action, string result);
    Task<Result<List<AuditLog>>> GetLogsAsync(string? entityId = null, string? entity = null);
}