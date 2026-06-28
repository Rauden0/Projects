using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.User;

public class UserRepository : Repository<Models.User>, IUserRepository
{
    private readonly BookHubDbContext _db;
    public UserRepository(BookHubDbContext db) : base(db) => _db = db;

    public Task<Models.User?> GetUserByEmailAsync(string email) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
}