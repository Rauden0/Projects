using api.Repository.TeamRepository;
using Microsoft.EntityFrameworkCore;
using TournamentBackEnd.Models.Team;
using TournamentBackEnd.TournamentDatabase;

namespace TournamentBackEnd.Repository.TeamRepository;

public class TeamRepository : ITeamRepository
{
    private readonly TournamentDbContext _dbContext;

    public TeamRepository(TournamentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Team>> GetAllTeamsAsync()
    {
        return await _dbContext.Teams.Include(c => c.Members).ToListAsync();
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        return await _dbContext.Teams.FirstOrDefaultAsync(i => i.Id == id);
        //return await _dbContext.Teams.Include(c => c.Members).FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Team?> GetTeamByNameAsync(string name)
    {
        return await _dbContext.Teams.FirstOrDefaultAsync(i => i.Name == name);
    }

    public async Task<int> CreateTeamAsync(Team? team)
    {
        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync();
        return team?.Id ?? 0;
    }

    public async Task<bool> UpdateTeamAsync(Team initialTeam, Team updatedTeam)
    {
        _dbContext.Entry(initialTeam).CurrentValues.SetValues(updatedTeam);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var team = await _dbContext.Teams.FindAsync(id);
        if (team == null) return false;

        await DisassociateUsersFromTeamAsync(id);
        _dbContext.Teams.Remove(team);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task DisassociateUsersFromTeamAsync(int teamId)
    {
        var users = await _dbContext.Users.Where(u => u.Team != null && u.Team.Id == teamId).ToListAsync();

        foreach (var user in users) user.Team = null;
    }
}