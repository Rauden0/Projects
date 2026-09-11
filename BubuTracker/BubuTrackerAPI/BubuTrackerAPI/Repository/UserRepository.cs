using BubuTrackerAPI.UserDatabase;
using BubuTrackerAPI.UserDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace BubuTrackerAPI.Repository;

public class UserRepository : IUserRepository
{
    private readonly BubuTrackerDbContext _context;

    public UserRepository(BubuTrackerDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByAuth0SubjectIdAsync(string auth0SubjectId) =>
        _context.Users.FirstOrDefaultAsync(u => u.Auth0SubjectId == auth0SubjectId);

    public Task<User?> GetByEmailAsync(string email) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByIdAsync(Guid id) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
