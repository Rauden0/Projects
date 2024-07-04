using api.Models.Enums;
using TournamentBackEnd.Models.Node;

namespace TournamentBackEnd.Models.Tournament;

public class Tournament
{
    public int Id { get; set; }
    public string Title { get; set; }
    public TournamentType TournamentType { get; set; }
    public List<Team.Team> Teams { get; set; } = new();
    public int? RootNodeId { get; set; }
    public string? Description { get; set; }
    public TournamentNode? RootNode { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public int Capacity { get; set; }
    public List<TournamentNode> TournamentNodes { get; set; }
}