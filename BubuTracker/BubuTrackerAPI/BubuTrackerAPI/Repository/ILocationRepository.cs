using BubuTrackerAPI.UserDatabase.Models;

namespace BubuTrackerAPI.Repository;

public interface ILocationRepository
{
    Task<Location?> GetByUserIdAsync(Guid userId);
    Task<Location> UpsertAsync(Guid userId, double latitude, double longitude);
    Task<IReadOnlyList<Location>> GetTrackedLocationsAsync(Guid trackerId);
}
