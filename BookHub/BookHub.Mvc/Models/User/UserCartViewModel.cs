using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Cart;

namespace BookHub.Mvc.Models.User;

public class UserCartViewModel
{
    public int Id { get; set; }
    public List<int> CartItemsIds { get; set; } = new();
    public List<CartItemDto> CartItems { get; set; } = new();
    public List<BookDto> Books { get; set; } = new();
}