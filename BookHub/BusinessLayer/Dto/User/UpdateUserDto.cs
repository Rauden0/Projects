using DataAccessLayer.Enums;

namespace BusinessLayer.Dto.User;

public class UpdateUserDto
{
    public int Id { get; set; }
    public string? DisplayName { get; set; }
    public RoleType? Role { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}