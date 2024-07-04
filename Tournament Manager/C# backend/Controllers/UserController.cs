using api.Dtos.User;
using api.Mappers;
using api.Repository.UserRepository;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto, [FromRoute] int? id)
    {
        var user = createUserDto.ToUserFromCreateDto();
        var userId = await _userRepository.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = userId }, user);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto userDto)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        user.FirstName = userDto.FirstName ?? "";
        user.LastName = userDto.LastName ?? "";

        await _userRepository.UpdateUserAsync(user);
        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        var success = await _userRepository.DeleteUserAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}