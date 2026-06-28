using Microsoft.AspNetCore.Identity;

namespace BookHub.Mvc.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}