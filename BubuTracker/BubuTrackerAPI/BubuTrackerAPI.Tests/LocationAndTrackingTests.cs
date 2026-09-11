using System.Net;
using System.Net.Http.Json;
using BubuTrackerAPI.Dtos;
using BubuTrackerAPI.UserDatabase;
using BubuTrackerAPI.UserDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BubuTrackerAPI.Tests;

public class LocationAndTrackingTests : IClassFixture<BubuTrackerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly BubuTrackerWebApplicationFactory _factory;

    public LocationAndTrackingTests(BubuTrackerWebApplicationFactory factory)
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
    public async Task TrackingFlow_AllowsFetchingTrackedLocation()
    {
        await SeedUser("auth0|tracker", "tracker@example.com", "Tracker", "One");
        await SeedUser("auth0|tracked", "tracked@example.com", "Tracked", "Two");

        var trackerAuth = "auth0|tracker::tracker@example.com";
        var trackedAuth = "auth0|tracked::tracked@example.com";

        var addTracking = CreateAuthedRequest(HttpMethod.Post, "/api/tracking", trackerAuth);
        addTracking.Content = JsonContent.Create(new AddTrackingDto { Email = "tracked@example.com" });
        var addResponse = await _client.SendAsync(addTracking);
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        var locationUpdate = CreateAuthedRequest(HttpMethod.Post, "/api/location", trackedAuth);
        locationUpdate.Content = JsonContent.Create(new LocationUpdateDto
        {
            Latitude = 47.5,
            Longitude = 19.0
        });
        var locationResponse = await _client.SendAsync(locationUpdate);
        Assert.Equal(HttpStatusCode.OK, locationResponse.StatusCode);

        var trackedLocations = CreateAuthedRequest(HttpMethod.Get, "/api/location/tracked", trackerAuth);
        var trackedResponse = await _client.SendAsync(trackedLocations);
        Assert.Equal(HttpStatusCode.OK, trackedResponse.StatusCode);

        var locations = await trackedResponse.Content.ReadFromJsonAsync<List<TrackedLocationDto>>();
        Assert.NotNull(locations);
        Assert.Single(locations!);
        Assert.Equal(47.5, locations![0].Latitude, precision: 3);
        Assert.Equal(19.0, locations[0].Longitude, precision: 3);
    }

    [Fact]
    public async Task AddTracking_ReturnsBadRequest_WhenTrackingSelf()
    {
        await SeedUser("auth0|solo", "solo@example.com", "Solo", "User");

        var request = CreateAuthedRequest(HttpMethod.Post, "/api/tracking", "auth0|solo::solo@example.com");
        request.Content = JsonContent.Create(new AddTrackingDto { Email = "solo@example.com" });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task SeedUser(string subjectId, string email, string firstName, string lastName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BubuTrackerDbContext>();
        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            return;
        }

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Auth0SubjectId = subjectId,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        });
        await db.SaveChangesAsync();
    }

    private static HttpRequestMessage CreateAuthedRequest(HttpMethod method, string url, string authHeader)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Test-User", authHeader);
        return request;
    }
}
