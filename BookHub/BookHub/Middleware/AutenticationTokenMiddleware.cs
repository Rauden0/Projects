using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace BookHub.Middleware;

public class AuthenticationTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationTokenMiddleware> _logger;
    private readonly string _expectedToken;

    public AuthenticationTokenMiddleware(RequestDelegate next, ILogger<AuthenticationTokenMiddleware> logger, IConfiguration cfg)
    {
        _next = next;
        _logger = logger;
        _expectedToken = cfg["Auth:Token"] ?? "";
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.Contains("swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(_expectedToken))
        {
            _logger.LogWarning("Auth token not set (Auth:Token). Blocking the request.");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Auth token not configured.");
            return;
        }

        var header = context.Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        var provided = header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? header[prefix.Length..].Trim()
            : "";

        if (provided != _expectedToken)
        {
            _logger.LogInformation("Unauthorized request to {Path}", path);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await _next(context);
    }
}
