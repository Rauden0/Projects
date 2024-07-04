namespace TournamentBackEnd.Models.Node;

public class TournamentNode
{
    public int Id { get; set; }
    public int Depth { get; set; }
    public int ScoreA { get; set; }
    public int ScoreB { get; set; }
    public bool Finished { get; set; }
    public int? SuccessorId { get; set; }
    public TournamentNode? Successor { get; set; } = null;
    public List<TournamentNode> Successors { get; set; }
    public List<Team.Team> Teams { get; set; } = new();
    public int TournamentIdUser { get; set; }

    public int TournamentId { get; set; }
    public Tournament.Tournament Tournament { get; set; }
}