using BusinessLayer.Service.Logging;
using DataAccessLayer.Models.Logging;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Controller;

[ApiController]
[Route("/audit")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AuditLog>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? entityId, [FromQuery] string? entity)
    {
        var logs = await _auditLogService.GetLogsAsync(entityId, entity);
        return logs.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
}