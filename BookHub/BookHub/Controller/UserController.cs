using BusinessLayer.Dto.User;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetUsers([FromQuery] ODataQueryOptions<UserDto>? options)
    {
        var users = await _service.GetUsersAsync(options);
        return users.Match<IActionResult>(Ok, ex => NotFound(ex.Message)
        );
    }

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserAsync(int id)
    {
        var item = await _service.GetUserAsync(id);
        return item.Match<IActionResult>(Ok, ex => NotFound(ex.Message));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    public async Task<ActionResult> CreateAsync([FromBody] CreateUserDto dto)
    {
        var user = await _service.CreateAsync(dto);
        return user.Match<ActionResult>(Ok, ex => BadRequest(ex.Message));
    }

    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _service.UpdateAsync(id, dto);
        return user.Match<IActionResult>(Ok, ex => NotFound(ex.Message));
    }

    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
    
    [HttpGet("test-audit")]
    public IActionResult TestAudit()
    {
        var auditId = User.FindFirst("Id")?.Value;
        return Ok(new { AuditId = auditId ?? "no-token" });
    }
}
