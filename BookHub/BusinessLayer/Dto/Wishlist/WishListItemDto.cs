namespace BusinessLayer.Dto.Wishlist;

public class WishListItemDto
{
    public int BookId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.Now;

}