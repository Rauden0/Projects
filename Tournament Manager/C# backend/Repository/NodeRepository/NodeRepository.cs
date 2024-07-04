using Microsoft.EntityFrameworkCore;
using TournamentBackEnd.Models.Node;
using TournamentBackEnd.TournamentDatabase;

namespace TournamentBackEnd.Repository.NodeRepository;

public class NodeRepository : INodeRepository
{
    private readonly TournamentDbContext _dbContext;

    public NodeRepository(TournamentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TournamentNode>> GetAllNodeAsync()
    {
        return await _dbContext.TournamentNodes.Include(n => n.Teams).Include(n => n.Successors).ToListAsync();
    }

    public async Task<TournamentNode?> GetNodeByIdAsync(int id)
    {
        return await _dbContext.TournamentNodes.Include(n => n.Teams).Include(n => n.Successors)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<TournamentNode?> UpdateNodeAsync(TournamentNode node)
    {
        _dbContext.Entry(node).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return node;
    }

    public async Task<int> CreateNodeAsync(TournamentNode node)
    {
        _dbContext.TournamentNodes.Add(node);
        await _dbContext.SaveChangesAsync();
        return node.Id;
    }

    public static async Task<TournamentNode> CreateTournamentNodeSystemAsync(int capacity, int tournamentId,
        TournamentDbContext dbContext,
        int depth = 0, int current = 0, TournamentNode? parentNode = null)
    {
        var newNode = new TournamentNode
        {
            SuccessorId = parentNode?.Id ?? null,
            Tournament = (await dbContext.Tournaments.FindAsync(tournamentId))!,
            Successors = new List<TournamentNode>(),
            Depth = depth
        };
        if (current >= capacity - 1) return newNode;

        await dbContext.TournamentNodes.AddAsync(newNode);
        await dbContext.SaveChangesAsync();

        if (depth > 0)
        {
            parentNode?.Successors.Add(newNode);
            await dbContext.SaveChangesAsync();
        }

        await CreateTournamentNodeSystemAsync(capacity, tournamentId, dbContext, depth + 1, 2 * current + 1, newNode);
        await CreateTournamentNodeSystemAsync(capacity, tournamentId, dbContext, depth + 1, 2 * current + 2, newNode);
        return newNode;
    }

    public static async Task CreateTournamentNodeSystemRoundRobinAsync(int capacity, int tournamentId,
        TournamentDbContext dbContext)
    {
        for (var i = 0; i < (capacity-1) * (capacity/2); i++)
        {
            var newNode = new TournamentNode
            {
                Tournament = (await dbContext.Tournaments.FindAsync(tournamentId))!
            };
            await dbContext.TournamentNodes.AddAsync(newNode);
        }

        await dbContext.SaveChangesAsync();
    }
}