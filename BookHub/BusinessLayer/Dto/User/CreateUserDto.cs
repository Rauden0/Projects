using DataAccessLayer.Enums;

namespace BusinessLayer.Dto.User;

public class CreateUserDto
{
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string Password { get; set; } = default!;

    public RoleType? Role { get; set; }
}