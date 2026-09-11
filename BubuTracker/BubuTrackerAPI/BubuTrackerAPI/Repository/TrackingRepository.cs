using BubuTrackerAPI.UserDatabase;
using BubuTrackerAPI.UserDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace BubuTrackerAPI.Repository;

public class TrackingRepository : ITrackingRepository
{
    private readonly BubuTrackerDbContext _context;

    public TrackingRepository(BubuTrackerDbContext context)
    {
        _context = context;
    }

    public Task<UserTracking?> GetAsync(Guid trackerId, Guid trackedUserId) =>
        _context.UserTrackings.FirstOrDefaultAsync(t =>
            t.TrackerId == trackerId && t.TrackedUserId == trackedUserId);

    public async Task<UserTracking> AddAsync(Guid trackerId, Guid trackedUserId)
    {
        var tracking = new UserTracking
        {
            TrackerId = trackerId,
            TrackedUserId = trackedUserId,
            CreatedAt = DateTime.UtcNow
        };
        _context.UserTrackings.Add(tracking);
        await _context.SaveChangesAsync();
        return tracking;
    }

    public async Task RemoveAsync(Guid trackerId, Guid trackedUserId)
    {
        var tracking = await GetAsync(trackerId, trackedUserId);
        if (tracking is null)
        {
            return;
        }

        _context.UserTrackings.Remove(tracking);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<User>> GetTrackedUsersAsync(Guid trackerId)
    {
        return await _context.UserTrackings
            .Where(t => t.TrackerId == trackerId)
            .Select(t => t.TrackedUser)
            .ToListAsync();
    }
}
