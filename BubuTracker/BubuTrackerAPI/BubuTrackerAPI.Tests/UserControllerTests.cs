using System.Net;
using System.Net.Http.Json;
using BubuTrackerAPI.Dtos;
using BubuTrackerAPI.UserDatabase;
using Microsoft.Extensions.DependencyInjection;

namespace BubuTrackerAPI.Tests;

public class UserControllerTests : IClassFixture<BubuTrackerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly BubuTrackerWebApplicationFactory _factory;

    public UserControllerTests(BubuTrackerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        EnsureDatabase();
    }

    private void EnsureDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BubuTrackerDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetMe_CreatesUser_WhenMissing()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/me");
        request.Headers.Add("X-Test-User", "auth0|user1::user1@example.com");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("user1@example.com", profile!.Email);
    }

    [Fact]
    public async Task UpdateMe_UpdatesProfile()
    {
        var authHeader = "auth0|user2::user2@example.com";
        await _client.SendAsync(CreateAuthedRequest(HttpMethod.Get, "/api/user/me", authHeader));

        var updateRequest = CreateAuthedRequest(HttpMethod.Put, "/api/user/me", authHeader);
        updateRequest.Content = JsonContent.Create(new UserUpdateDto
        {
            FirstName = "Alice",
            LastName = "Smith"
        });

        var response = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.Equal("Alice", profile!.FirstName);
        Assert.Equal("Smith", profile.LastName);
    }

    private static HttpRequestMessage CreateAuthedRequest(HttpMethod method, string url, string authHeader)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Test-User", authHeader);
        return request;
    }
}
