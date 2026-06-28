namespace DataAccessLayer.Models;

public class WishList : BaseEntity
{
    public int UserId { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

    public string Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}