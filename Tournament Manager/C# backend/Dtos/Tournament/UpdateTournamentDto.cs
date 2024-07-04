using api.Models.Enums;

namespace api.Dtos.Tournament;

public class UpdateTournamentDto
{
    public string Title { get; set; }
    public TournamentType TournamentType { get; set; }
    public string? Description { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<TournamentBackEnd.Models.Team.Team>? Teams { get; set; }
}