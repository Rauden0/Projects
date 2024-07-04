using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TournamentBackEnd.Models.Node;
using TournamentManagaer.Entities;

namespace TournamentBackEnd.Models.Team;

[Index("Name", IsUnique = true)]
public class Team
{
    public int Id { get; set; }

    [Required] public string Name { get; set; }

    public string? Description { get; set; }
    public List<TournamentNode> TournamentsAsAnAtendee { get; set; }
    public List<User> Members { get; set; } = new();
}