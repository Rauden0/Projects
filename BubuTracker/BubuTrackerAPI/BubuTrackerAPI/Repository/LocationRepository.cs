using BubuTrackerAPI.UserDatabase;
using BubuTrackerAPI.UserDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace BubuTrackerAPI.Repository;

public class LocationRepository : ILocationRepository
{
    private readonly BubuTrackerDbContext _context;

    public LocationRepository(BubuTrackerDbContext context)
    {
        _context = context;
    }

    public Task<Location?> GetByUserIdAsync(Guid userId) =>
        _context.Locations.FirstOrDefaultAsync(l => l.UserId == userId);

    public async Task<Location> UpsertAsync(Guid userId, double latitude, double longitude)
    {
        var existing = await _context.Locations.FirstOrDefaultAsync(l => l.UserId == userId);
        if (existing is null)
        {
            existing = new Location
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Latitude = latitude,
                Longitude = longitude,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Locations.Add(existing);
        }
        else
        {
            existing.Latitude = latitude;
            existing.Longitude = longitude;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.Locations.Update(existing);
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<IReadOnlyList<Location>> GetTrackedLocationsAsync(Guid trackerId)
    {
        var trackedUserIds = await _context.UserTrackings
            .Where(t => t.TrackerId == trackerId)
            .Select(t => t.TrackedUserId)
            .ToListAsync();

        return await _context.Locations
            .Include(l => l.User)
            .Where(l => trackedUserIds.Contains(l.UserId))
            .ToListAsync();
    }
}
