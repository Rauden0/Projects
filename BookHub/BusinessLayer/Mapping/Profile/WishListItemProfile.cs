using BusinessLayer.Dto.Wishlist;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class WishListItemProfile : AutoMapper.Profile
{
    public WishListItemProfile()
    {
        CreateMap<WishlistItem, WishListItemDto>();
    }
}