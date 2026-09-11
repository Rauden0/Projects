using BubuTrackerAPI.UserDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace BubuTrackerAPI.UserDatabase;

public class BubuTrackerDbContext : DbContext
{
    public BubuTrackerDbContext(DbContextOptions<BubuTrackerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<UserTracking> UserTrackings => Set<UserTracking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Auth0SubjectId).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Location)
                .WithOne(l => l.User)
                .HasForeignKey<Location>(l => l.UserId);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasIndex(l => l.UserId).IsUnique();
        });

        modelBuilder.Entity<UserTracking>(entity =>
        {
            entity.HasKey(t => new { t.TrackerId, t.TrackedUserId });
            entity.HasOne(t => t.Tracker)
                .WithMany(u => u.Tracking)
                .HasForeignKey(t => t.TrackerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.TrackedUser)
                .WithMany(u => u.TrackedBy)
                .HasForeignKey(t => t.TrackedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
