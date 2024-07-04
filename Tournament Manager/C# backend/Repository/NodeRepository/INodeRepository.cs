using TournamentBackEnd.Models.Node;

namespace TournamentBackEnd.Repository.NodeRepository;

public interface INodeRepository
{
    Task<List<TournamentNode>> GetAllNodeAsync();
    Task<TournamentNode?> GetNodeByIdAsync(int id);
    Task<TournamentNode?> UpdateNodeAsync(TournamentNode node);
}