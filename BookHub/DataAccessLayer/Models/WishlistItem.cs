namespace DataAccessLayer.Models;

public class WishlistItem : BaseEntity
{
    public int WishlistId { get; set; }

    public virtual WishList Wishlist { get; set; }

    public int BookId { get; set; }

    public virtual Book Book { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.Now;
}