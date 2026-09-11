using BubuTrackerAPI.Dtos;
using BubuTrackerAPI.Repository;
using BubuTrackerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubuTrackerAPI.Controllers;

[Route("api/location")]
[ApiController]
[Authorize]
public class LocationController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILocationRepository _locationRepository;

    public LocationController(
        ICurrentUserService currentUserService,
        ILocationRepository locationRepository)
    {
        _currentUserService = currentUserService;
        _locationRepository = locationRepository;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDto dto)
    {
        if (dto.Latitude is < -90 or > 90 || dto.Longitude is < -180 or > 180)
        {
            return BadRequest(new { message = "Invalid coordinates." });
        }

        var user = await _currentUserService.GetOrCreateCurrentUserAsync();
        await _locationRepository.UpsertAsync(user.Id, dto.Latitude, dto.Longitude);
        return Ok(new { message = "Location updated." });
    }

    [HttpGet("tracked")]
    public async Task<ActionResult<IEnumerable<TrackedLocationDto>>> GetTrackedLocations()
    {
        var user = await _currentUserService.GetOrCreateCurrentUserAsync();
        var locations = await _locationRepository.GetTrackedLocationsAsync(user.Id);

        var result = locations.Select(l => new TrackedLocationDto
        {
            UserId = l.UserId,
            Email = l.User.Email,
            FirstName = l.User.FirstName,
            LastName = l.User.LastName,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            UpdatedAt = l.UpdatedAt
        });

        return Ok(result);
    }
}
