using BubuTrackerAPI.UserDatabase.Models;

namespace BubuTrackerAPI.Repository;

public interface IUserRepository
{
    Task<User?> GetByAuth0SubjectIdAsync(string auth0SubjectId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
}
