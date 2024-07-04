using api.Models.Enums;

namespace api.Dtos.Tournament;

public class CreateTournamentDto
{
    public string Title { get; set; }
    public TournamentType TournamentType { get; set; }
    public string? Description { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public int Capacity { get; set; }
    public List<TournamentManagaer.Entities.User>? Users { get; set; }
    public List<TournamentManagaer.Entities.User>? AdminUsers { get; set; }
}