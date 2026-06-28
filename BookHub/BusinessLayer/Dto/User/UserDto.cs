namespace BusinessLayer.Dto.User;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = default!;
}