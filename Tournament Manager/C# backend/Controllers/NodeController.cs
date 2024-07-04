using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TournamentBackEnd.Dtos.Node;
using TournamentBackEnd.Repository.NodeRepository;

namespace TournamentBackEnd.Controllers;

[Route("api/node")]
[ApiController]
public class NodeController : ControllerBase
{
    private readonly INodeRepository _nodeRepository;

    public NodeController(INodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var nodes = await _nodeRepository.GetAllNodeAsync();
        return Ok(nodes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var node = await _nodeRepository.GetNodeByIdAsync(id);
        if (node == null) return NotFound();
        return Ok(node);
    }

    [HttpPatch("/advance/{id:int}")]
    public async Task<IActionResult> UpdateNodeAdvance([FromRoute] int id, [FromBody] UpdateNodeDto nodeDto)
    {
        var node = await _nodeRepository.GetNodeByIdAsync(id);
        if (node == null) return NotFound();

        if (node.Teams.Count != 2) return Content("Not enought teams");

        var team = node.Teams.Find(x => x.Id == nodeDto.TeamId || x.Name == nodeDto.TeamName);
        if (team == null) return NotFound();
        if (Equals(node.Teams[0], team)) node.ScoreA = 1;
        if (Equals(node.Teams[1], team)) node.ScoreB = 1;
        await _nodeRepository.UpdateNodeAsync(node);

        if (node.SuccessorId == null) return Content("Already in Final Round");

        var nextNode = await _nodeRepository.GetNodeByIdAsync(node.SuccessorId ?? 0);
        if (nextNode == null) return NotFound();
        if (nextNode.Teams.Count == 2) return Content("Full bracket");
        if (nextNode.Teams.Any(x => team.Name == x.Name)) return Content("Already advanced");
        if (Equals(node.Teams[0], team))
        {
            nextNode.Teams.Add(team);
            node.ScoreA = 1;
        }

        if (Equals(node.Teams[1], team))
        {
            nextNode.Teams.Add(team);
            node.ScoreB = 1;
        }

        await _nodeRepository.UpdateNodeAsync(nextNode);
        node.Finished = true;
        await _nodeRepository.UpdateNodeAsync(node);

        return Ok(nextNode);
    }

    [HttpPut("/regress/{id:int}")]
    public async Task<IActionResult> UpdateNodeRegress([FromRoute] int id, [FromBody] UpdateNodeDto nodeDto)
    {
        var node = await _nodeRepository.GetNodeByIdAsync(id);
        if (node == null) return NotFound();
        var team = node.Teams.Find(x => x.Id == nodeDto.TeamId || x.Name == nodeDto.TeamName);

        if (team == null) return NotFound();
        if (node.Successors.IsNullOrEmpty()) return Content("Cant regress in a last spot");
        foreach (var teamI in node.Teams.ToList())
            if (Equals(teamI, team))
                node.Teams.Remove(team);
        await _nodeRepository.UpdateNodeAsync(node);

        return Ok(node);
    }
}