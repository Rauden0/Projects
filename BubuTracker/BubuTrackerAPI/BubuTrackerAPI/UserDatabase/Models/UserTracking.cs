namespace BubuTrackerAPI.UserDatabase.Models;

public class UserTracking
{
    public Guid TrackerId { get; set; }
    public Guid TrackedUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Tracker { get; set; } = null!;
    public User TrackedUser { get; set; } = null!;
}
