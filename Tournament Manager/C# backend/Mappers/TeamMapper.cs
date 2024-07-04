using api.Dtos.Team;
using TournamentBackEnd.Models.Team;

namespace TournamentBackEnd.Mappers;

public static class TeamMapper
{
    public static Team ToTeamFromCreateDto(this CreateTeamDto teamDto)
    {
        return new Team
        {
            Name = teamDto.Name,
            Description = teamDto.Description
        };
    }

    public static Team ToTeamFromUpdateDto(this UpdateTeamDto teamDto)
    {
        var tournament = new Team
        {
            Name = teamDto.Name,
            Description = teamDto.Description
        };

        return tournament;
    }
}