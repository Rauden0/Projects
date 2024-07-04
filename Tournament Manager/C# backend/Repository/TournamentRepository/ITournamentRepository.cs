using TournamentBackEnd.Models.Node;
using TournamentBackEnd.Models.Team;
using TournamentBackEnd.Models.Tournament;

namespace api.Repository.Interfaces;

public interface ITournamentRepository
{
    Task<List<Tournament>> GetAllTournamentsAsync();
    Task<Tournament?> GetTournamentByIdAsync(int id);
    Task<int> CreateTournamentAsync(Tournament tournament);
    Task<bool> UpdateTournamentAsync(Tournament tournament);
    Task<bool> DeleteTournamentAsync(int id);
    Task<Team?> GetTeamByIdAsync(int? id, string? name);
    Task<TournamentNode?> UpdateTournamentNodeAsync(TournamentNode node);
}