namespace BubuTrackerAPI.UserDatabase.Models;

public class User
{
    public Guid Id { get; set; }
    public string Auth0SubjectId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Location? Location { get; set; }
    public ICollection<UserTracking> Tracking { get; set; } = new List<UserTracking>();
    public ICollection<UserTracking> TrackedBy { get; set; } = new List<UserTracking>();
}
