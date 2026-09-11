using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BubuTrackerAPI.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-User", out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Test-User header."));
        }

        var parts = headerValues.ToString().Split("::", 2, StringSplitOptions.None);
        if (parts.Length < 2)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid X-Test-User header."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, parts[0]),
            new Claim("sub", parts[0]),
            new Claim(ClaimTypes.Email, parts[1])
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
