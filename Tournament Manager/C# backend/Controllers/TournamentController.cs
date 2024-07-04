using api.Dtos.Tournament;
using api.Mappers;
using api.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TournamentBackEnd.Dtos.Tournament;
using TournamentBackEnd.Models.Tournament;

namespace TournamentBackEnd.Controllers;

[Route("api/tournament")]
[ApiController]
public class TournamentController : Controller
{
    private readonly ITournamentRepository _tournamentRepository;

    public TournamentController(ITournamentRepository tournamentRepository)
    {
        _tournamentRepository = tournamentRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<Tournament?> tournaments = await _tournamentRepository.GetAllTournamentsAsync();
        return Ok(tournaments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var tournament = await _tournamentRepository.GetTournamentByIdAsync(id);
        if (tournament == null) return NotFound();
        return Ok(tournament);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentDto tournamentDto)
    {
        var tournament = tournamentDto.ToTournamentFromCreateDto();
        await _tournamentRepository.CreateTournamentAsync(tournament);
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTournament([FromRoute] int id, [FromBody] UpdateTournamentDto tournamentDto)
    {
        var tournament = await _tournamentRepository.GetTournamentByIdAsync(id);
        if (tournament == null) return NotFound();

        tournament.TournamentType = tournamentDto.TournamentType;
        tournament.Description = tournamentDto.Description ?? "";
        tournament.StartDate = tournamentDto.StartDate;
        tournament.EndDate = tournamentDto.EndDate;

        await _tournamentRepository.UpdateTournamentAsync(tournament);
        return Ok(tournament);
    }

    [HttpPut("/team/{id:int}")]
    public async Task<IActionResult> AddTeamToTournament([FromRoute] int id,
        [FromBody] AddTeamToTournamentDto tournamentDto)
    {
        var tournament = await _tournamentRepository.GetTournamentByIdAsync(id);
        if (tournament == null) return NotFound();
        var nodes = tournament.TournamentNodes.FindAll(x => x.Successors.Count == 0 && x.Teams.Count < 2);

        var team = await _tournamentRepository.GetTeamByIdAsync(tournamentDto.TeamId, tournamentDto.TeamName);
        if (team == null) return NotFound();

        foreach (var node in tournament.TournamentNodes)
            if (node.Teams.Exists(t => t.Name == team.Name))
                return Content("Duplicate found");
        if (nodes.IsNullOrEmpty()) return Content("Full tournament");
        nodes.First().Teams.Add(team);
        await _tournamentRepository.UpdateTournamentNodeAsync(nodes.First());
        return Ok(nodes.First());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTournament([FromRoute] int id)
    {
        var success = await _tournamentRepository.DeleteTournamentAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("/team/{id:int}")]
    public async Task<IActionResult> DeleteTeam([FromRoute] int id, RemoveTeamInTournamentDto tournamentDto)
    {
        var tournament = await _tournamentRepository.GetTournamentByIdAsync(id);
        foreach (var node in tournament.TournamentNodes)
        {
            var membersToRemove = node.Teams
                .Where(t => tournamentDto.TeamName == t.Name || tournamentDto.TeamId == t.Id)
                .ToList();

            foreach (var memberToRemove in membersToRemove)
            {
                node.Teams.Remove(memberToRemove);
                await _tournamentRepository.UpdateTournamentNodeAsync(node);
            }
        }

        await _tournamentRepository.UpdateTournamentAsync(tournament);
        return Ok();
    }
}