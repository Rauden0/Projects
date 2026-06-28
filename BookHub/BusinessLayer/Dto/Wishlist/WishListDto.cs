namespace BusinessLayer.Dto.Wishlist;

public class WishListDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int UserId { get; set; }
    public List<int> BookIds { get; set; }
}