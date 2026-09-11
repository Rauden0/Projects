using BubuTrackerAPI.Dtos;
using BubuTrackerAPI.Repository;
using BubuTrackerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubuTrackerAPI.Controllers;

[Route("api/tracking")]
[ApiController]
[Authorize]
public class TrackingController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly ITrackingRepository _trackingRepository;

    public TrackingController(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        ITrackingRepository trackingRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _trackingRepository = trackingRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetTrackedUsers()
    {
        var user = await _currentUserService.GetOrCreateCurrentUserAsync();
        var trackedUsers = await _trackingRepository.GetTrackedUsersAsync(user.Id);

        return Ok(trackedUsers.Select(u => new UserProfileDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName
        }));
    }

    [HttpPost]
    public async Task<IActionResult> AddTracking([FromBody] AddTrackingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "Email is required." });
        }

        var tracker = await _currentUserService.GetOrCreateCurrentUserAsync();
        var trackedUser = await _userRepository.GetByEmailAsync(dto.Email.Trim());

        if (trackedUser is null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (trackedUser.Id == tracker.Id)
        {
            return BadRequest(new { message = "You cannot track yourself." });
        }

        var existing = await _trackingRepository.GetAsync(tracker.Id, trackedUser.Id);
        if (existing is not null)
        {
            return Conflict(new { message = "Already tracking this user." });
        }

        await _trackingRepository.AddAsync(tracker.Id, trackedUser.Id);
        return Ok(new { message = "User added to tracking list." });
    }

    [HttpDelete("{trackedUserId:guid}")]
    public async Task<IActionResult> RemoveTracking(Guid trackedUserId)
    {
        var tracker = await _currentUserService.GetOrCreateCurrentUserAsync();
        await _trackingRepository.RemoveAsync(tracker.Id, trackedUserId);
        return NoContent();
    }
}
