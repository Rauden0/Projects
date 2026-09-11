using BubuTrackerAPI.Repository;
using BubuTrackerAPI.UserDatabase;
using BubuTrackerAPI.UserDatabase.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BubuTrackerAPI.Tests;

public class LocationRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BubuTrackerDbContext _context;
    private readonly LocationRepository _repository;

    public LocationRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<BubuTrackerDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new BubuTrackerDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new LocationRepository(_context);
    }

    [Fact]
    public async Task UpsertAsync_CreatesThenUpdatesLocation()
    {
        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId,
            Auth0SubjectId = "auth0|test",
            Email = "test@example.com"
        });
        await _context.SaveChangesAsync();

        await _repository.UpsertAsync(userId, 10.0, 20.0);
        var created = await _repository.GetByUserIdAsync(userId);

        Assert.NotNull(created);
        Assert.Equal(10.0, created!.Latitude);
        Assert.Equal(20.0, created.Longitude);

        await _repository.UpsertAsync(userId, 11.0, 21.0);
        var updated = await _repository.GetByUserIdAsync(userId);

        Assert.Equal(11.0, updated!.Latitude);
        Assert.Equal(21.0, updated.Longitude);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
