using api.Dtos.Team;
using api.Repository.TeamRepository;
using api.Repository.UserRepository;
using Microsoft.AspNetCore.Mvc;
using TournamentBackEnd.Mappers;
using TournamentBackEnd.Models.Team;

namespace TournamentBackEnd.Controllers;

[Route("api/team")]
[ApiController]
public class TeamController : ControllerBase
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUserRepository _userRepository;

    public TeamController(ITeamRepository teamRepository, IUserRepository userRepository)
    {
        _teamRepository = teamRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teams = await _teamRepository.GetAllTeamsAsync();
        return Ok(teams);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        Team? team;
        team = await _teamRepository.GetTeamByIdAsync(id);
        if (team == null) return NotFound();
        return Ok(team);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto teamDto)
    {
        var team = teamDto.ToTeamFromCreateDto();
        var duplicate = await _teamRepository.GetTeamByNameAsync(teamDto.Name);
        if (duplicate != null) return BadRequest("Duplicate found");
        var teamId = await _teamRepository.CreateTeamAsync(team);
        return CreatedAtAction(nameof(GetById), new { id = teamId }, team);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateTeam([FromRoute] int id, [FromBody] UpdateTeamDto teamDto)
    {
        var team = await _teamRepository.GetTeamByIdAsync(id);
        if (team == null) return NotFound();
        var updatedTeam = new Team();
        updatedTeam.Id = id;
        updatedTeam.Name = teamDto.Name;
        await _teamRepository.UpdateTeamAsync(team, updatedTeam);
        return Ok(team);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTeam([FromRoute] int id)
    {
        var success = await _teamRepository.DeleteTeamAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPut("{teamId:int}/adduser")]
    public async Task<IActionResult> AddUser([FromRoute] int teamId, [FromBody] AddUserDto userDto)
    {
        var team = await _teamRepository.GetTeamByIdAsync(teamId);
        if (team == null) return NotFound();
        var user = await _userRepository.GetUserByIdAsync(userDto.UserId);
        if (user == null) return NotFound("User not found");

        team.Members.Add(user);
        await _teamRepository.UpdateTeamAsync(team, team);
        return Ok(team);
    }
}