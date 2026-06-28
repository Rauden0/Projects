using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BookHub.Api.Middleware;

public class JwtTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtTokenMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public JwtTokenMiddleware(
        RequestDelegate next,
        ILogger<JwtTokenMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["X-JWT"].FirstOrDefault();
        if (string.IsNullOrEmpty(token))
        {
            await _next(context);
            return;
        }

        try
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
            
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, tokenValidationParameters, out _);
            
            context.User = principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed (optional).");
        }
        await _next(context);
    }
}
