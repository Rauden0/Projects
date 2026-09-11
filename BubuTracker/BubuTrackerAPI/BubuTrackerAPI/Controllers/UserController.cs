using BubuTrackerAPI.Dtos;
using BubuTrackerAPI.Repository;
using BubuTrackerAPI.Services;
using BubuTrackerAPI.UserDatabase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubuTrackerAPI.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public UserController(ICurrentUserService currentUserService, IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMe()
    {
        var user = await _currentUserService.GetOrCreateCurrentUserAsync();
        return Ok(ToDto(user));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe([FromBody] UserUpdateDto updateDto)
    {
        var user = await _currentUserService.GetOrCreateCurrentUserAsync();

        if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
        {
            user.FirstName = updateDto.FirstName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(updateDto.LastName))
        {
            user.LastName = updateDto.LastName.Trim();
        }

        var updated = await _userRepository.UpdateAsync(user);
        return Ok(ToDto(updated));
    }

    private static UserProfileDto ToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName
    };
}
