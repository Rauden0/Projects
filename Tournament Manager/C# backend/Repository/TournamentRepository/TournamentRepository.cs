using api.Models.Enums;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using TournamentBackEnd.Models.Node;
using TournamentBackEnd.Models.Team;
using TournamentBackEnd.Models.Tournament;
using TournamentBackEnd.TournamentDatabase;

namespace TournamentBackEnd.Repository.TournamentRepository;

public class TournamentRepository : ITournamentRepository
{
    private readonly TournamentDbContext _dbContext;

    public TournamentRepository(TournamentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Tournament>> GetAllTournamentsAsync()
    {
        return await _dbContext.Tournaments.Include(c => c.Teams).ToListAsync();
    }

    public async Task<Team?> GetTeamByIdAsync(int? id, string? name)
    {
        return await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == id || t.Name == name);
    }

    public async Task<Tournament?> GetTournamentByIdAsync(int id)
    {
        var tournament = await _dbContext.Tournaments
            .Include(t => t.RootNode)
            .Include(t => t.TournamentNodes)
            .ThenInclude(n => n.Teams)
            .Include(t => t.TournamentNodes)
            .ThenInclude(n => n.Successors)
            .FirstOrDefaultAsync(i => i.Id == id);

        return tournament;
    }

    public async Task<int> CreateTournamentAsync(Tournament tournament)
    {
        _dbContext.Tournaments.Add(tournament);
        await _dbContext.SaveChangesAsync();
        TournamentNode? node = null;
        if (tournament.TournamentType == TournamentType.RoundRobin)
            await NodeRepository.NodeRepository.CreateTournamentNodeSystemRoundRobinAsync(tournament.Capacity,
                tournament.Id, _dbContext);
        else
            node = await NodeRepository.NodeRepository.CreateTournamentNodeSystemAsync(tournament.Capacity,
                tournament.Id, _dbContext);
        tournament.RootNodeId = node?.Id;
        tournament.RootNode = node;
        await UpdateTournamentAsync(tournament);
        await _dbContext.SaveChangesAsync();
        return tournament?.Id ?? 0;
    }

    public async Task<bool> UpdateTournamentAsync(Tournament tournament)
    {
        _dbContext.Entry(tournament).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTournamentAsync(int id)
    {
        var tournament = await _dbContext.Tournaments.FindAsync(id);
        if (tournament == null)
            return false;

        _dbContext.Tournaments.Remove(tournament);
        return true;
    }

    public async Task<TournamentNode?> UpdateTournamentNodeAsync(TournamentNode node)
    {
        _dbContext.Entry(node).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return node;
    }
}