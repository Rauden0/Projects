using api.Dtos.Tournament;
using TournamentBackEnd.Models.Team;
using TournamentBackEnd.Models.Tournament;

namespace api.Mappers;

public static class TournamentMappers
{
    public static Tournament ToTournamentFromCreateDto(this CreateTournamentDto tournamentDto)
    {
        return new Tournament
        {
            Title = tournamentDto.Title,
            TournamentType = tournamentDto.TournamentType,
            StartDate = tournamentDto.StartDate,
            EndDate = tournamentDto.EndDate,
            Capacity = tournamentDto.Capacity,
            Description = tournamentDto.Description ?? ""
        };
    }

    public static Tournament ToTournamentFromUpdateDto(this UpdateTournamentDto tournamentDto)
    {
        var tournament = new Tournament
        {
            Title = tournamentDto.Title,
            TournamentType = tournamentDto.TournamentType,
            Description = tournamentDto.Description,
            StartDate = tournamentDto.StartDate,
            EndDate = tournamentDto.EndDate,
            Teams = tournamentDto.Teams ?? new List<Team>()
        };

        return tournament;
    }
}