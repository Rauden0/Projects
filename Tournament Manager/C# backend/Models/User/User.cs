using TournamentBackEnd.Models.Team;

namespace TournamentManagaer.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Info { get; set; }
    public Team? Team { get; set; }
}