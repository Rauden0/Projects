using api.Dtos.Team;
using Microsoft.EntityFrameworkCore;
using TournamentBackEnd.Models.Node;
using TournamentBackEnd.Models.Team;
using TournamentBackEnd.Models.Tournament;
using TournamentManagaer.Entities;

namespace TournamentBackEnd.TournamentDatabase;

public class TournamentDbContext : DbContext
{
    public TournamentDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TournamentNode> TournamentNodes { get; set; }
    public DbSet<Team> Teams { get; set; }

    public DbSet<CreateTeamDto> CreateTeamDto { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TournamentNode>()
            .HasOne(tn => tn.Tournament)
            .WithMany(t => t.TournamentNodes)
            .HasForeignKey(tn => tn.TournamentId)
            .IsRequired();
        modelBuilder.Entity<TournamentNode>()
            .HasOne(node => node.Successor)
            .WithMany()
            .HasForeignKey(node => node.SuccessorId);

        modelBuilder.Entity<Tournament>()
            .HasOne(t => t.RootNode)
            .WithOne()
            .HasForeignKey<Tournament>(t => t.RootNodeId)
            .IsRequired(false);

        /* modelBuilder.Entity<TeamUser>().HasOne(u => u.Team)
             .WithMany(t => t.members).HasForeignKey(p => p.UserId);*/
    }
}