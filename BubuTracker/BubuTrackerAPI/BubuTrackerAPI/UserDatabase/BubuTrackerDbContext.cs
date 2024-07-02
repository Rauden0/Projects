using BubuTrackerAPI.UserDatabase.Models;

namespace BubuTrackerAPI.UserDatabase;
using Microsoft.EntityFrameworkCore;
public class UserDbContext:DbContext
{
    public UserDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }
    
    public DbSet<User> Users { get; set; }
}
