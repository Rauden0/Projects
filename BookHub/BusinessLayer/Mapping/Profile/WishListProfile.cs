using BusinessLayer.Dto.Wishlist;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class WishListMappingProfile : AutoMapper.Profile
{
    public WishListMappingProfile()
    {
        CreateMap<WishList, WishListDto>()
            // map through the WishlistItems relationship to get BookIds
            .ForMember(d => d.BookIds,
                opt => opt.MapFrom(s => s.WishlistItems.Select(wi => wi.BookId)));
    }
}