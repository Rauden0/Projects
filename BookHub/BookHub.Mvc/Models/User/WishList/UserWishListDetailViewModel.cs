using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Wishlist;

namespace BookHub.Mvc.Models.User.WishList;

public class UserWishListDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public List<WishListItemDto> WishListItems { get; set; } = new();
    public List<BookDto> Books { get; set; } = new();
}