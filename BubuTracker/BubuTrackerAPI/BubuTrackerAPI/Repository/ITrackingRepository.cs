using BubuTrackerAPI.UserDatabase.Models;

namespace BubuTrackerAPI.Repository;

public interface ITrackingRepository
{
    Task<UserTracking?> GetAsync(Guid trackerId, Guid trackedUserId);
    Task<UserTracking> AddAsync(Guid trackerId, Guid trackedUserId);
    Task RemoveAsync(Guid trackerId, Guid trackedUserId);
    Task<IReadOnlyList<User>> GetTrackedUsersAsync(Guid trackerId);
}
