using BusinessLayer.Dto.Logging;
using DataAccessLayer.Models.Logging;
using Nest;

namespace BusinessLayer.Service.Logging;

public class RequestLogService : IRequestLogService
{
    private readonly IElasticClient _client;
    private const string Index = "requests-log";

    public RequestLogService(IElasticClient client)
    {
        _client = client;
    }

    public async Task LogAsync(RequestLogDto logDto)
    {
        var log = new RequestLog
        {
            Source = logDto.Source,
            User = logDto.User,
            Method = logDto.Method,
            Path = logDto.Path,
            Query = logDto.Query,
            StatusCode = logDto.StatusCode,
            DurationMs = logDto.DurationMs
        };
        await _client.IndexAsync(log, i => i.Index(Index));
    }
}