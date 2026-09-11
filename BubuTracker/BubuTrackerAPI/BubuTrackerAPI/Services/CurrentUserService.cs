using System.Security.Claims;
using BubuTrackerAPI.Repository;
using BubuTrackerAPI.UserDatabase.Models;

namespace BubuTrackerAPI.Services;

public interface ICurrentUserService
{
    Task<User> GetOrCreateCurrentUserAsync();
    string? GetAuth0SubjectId();
    string? GetEmail();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public string? GetAuth0SubjectId() =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

    public string? GetEmail() =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("email");

    public async Task<User> GetOrCreateCurrentUserAsync()
    {
        var subjectId = GetAuth0SubjectId()
            ?? throw new UnauthorizedAccessException("Missing Auth0 subject claim.");
        var email = GetEmail() ?? $"{subjectId}@users.auth0.local";

        var user = await _userRepository.GetByAuth0SubjectIdAsync(subjectId);
        if (user is not null)
        {
            if (!string.IsNullOrWhiteSpace(GetEmail()) && user.Email != email)
            {
                user.Email = email;
                await _userRepository.UpdateAsync(user);
            }
            return user;
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            Auth0SubjectId = subjectId,
            Email = email,
            FirstName = GetClaimValue("given_name") ?? string.Empty,
            LastName = GetClaimValue("family_name") ?? string.Empty
        };

        return await _userRepository.CreateAsync(user);
    }

    private string? GetClaimValue(string claimType) =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
}
