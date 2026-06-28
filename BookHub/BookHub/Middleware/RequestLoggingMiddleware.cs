using System.Diagnostics;
using BusinessLayer.Dto.Logging;
using BusinessLayer.Service.Logging;

namespace BookHub.Middleware;

public class RequestLoggingMiddleware
{
    private const string Source = "API";
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly IRequestLogService _requestLogService;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IRequestLogService requestLogService)
    {
        _next = next;
        _logger = logger;
        _requestLogService = requestLogService;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var user = context.User.FindFirst("Id")?.Value ?? "unknown";
        var method = context.Request.Method;
        var path = context.Request.Path;
        var query = context.Request.QueryString.ToString();

        await _next(context);
        stopwatch.Stop();
        
        var statusCode = context.Response.StatusCode;

        _logger.LogInformation($"[{statusCode}] {method} {path}{query}");

        var requestLogDto = new RequestLogDto
        {
            Source = Source,
            User = user,
            Method = method,
            Path = path,
            Query = query,
            StatusCode = statusCode,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
        
        await _requestLogService.LogAsync(requestLogDto);
    }
}