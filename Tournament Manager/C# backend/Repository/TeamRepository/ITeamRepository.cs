using TournamentBackEnd.Models.Team;

namespace api.Repository.TeamRepository;

public interface ITeamRepository
{
    Task<List<Team>> GetAllTeamsAsync();
    Task<Team?> GetTeamByIdAsync(int id);
    Task<Team?> GetTeamByNameAsync(string name);
    Task<int> CreateTeamAsync(Team? Team);
    Task<bool> UpdateTeamAsync(Team initialTeam, Team updateTeam);
    Task<bool> DeleteTeamAsync(int id);
}