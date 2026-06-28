using BusinessLayer.Dto.Cart;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class CartItemProfile : AutoMapper.Profile
{
    public CartItemProfile()
    {
        CreateMap<CartItem, CartItemDto>();
    }
}