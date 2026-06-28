using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Models.User.WishList;

public class UserWishListViewModel
{
    public IEnumerable<SelectListItem>? WishLists { get; set; }
    public List<UserWishListDetailViewModel> WishListDetailViewModels { get; set; } = new();
}