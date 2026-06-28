using DataAccessLayer.Enums;

namespace DataAccessLayer.Models;

public class User : BaseEntity
{
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? DisplayName { get; set; }
    public RoleType Role { get; set; } = RoleType.User;   // enum v Enums/RoleType.cs

    public virtual Cart? Cart { get; set; }
    public virtual ICollection<Review>? Reviews { get; set; }
    public virtual ICollection<WishList>? WishLists { get; set; }
}