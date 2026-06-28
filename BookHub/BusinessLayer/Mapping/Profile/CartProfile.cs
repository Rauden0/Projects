using BusinessLayer.Dto.Cart;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class CartMappingProfile : AutoMapper.Profile
{
    public CartMappingProfile()
    {
        CreateMap<Cart, CartDto>()
            .ForMember(d => d.CartItemsIds,
                opt => opt.MapFrom(s => s.CartItems.Select(ci => ci.Id)))
            .ForMember(dest => dest.DiscountAmount,
                opt => opt.MapFrom(src => src.AppliedCouponId != null ? src.AppliedCoupon!.GiftCard.ReductionAmount : 0))
            .ForMember(dest => dest.RawTotalPrice,
                opt => opt.MapFrom(src => src.CartItems.Sum(ci => ci.Quantity * (decimal)ci.Book.Price)));
    }
}