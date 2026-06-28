using BusinessLayer.Dto.Logging;

namespace BusinessLayer.Service.Logging;

public interface IRequestLogService
{
    Task LogAsync(RequestLogDto log);
}