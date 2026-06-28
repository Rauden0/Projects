using BusinessLayer.Dto.Cart;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class CartItemMapper
{
    public static CartItemDto ToDto(CartItem entity) =>
        EntityMapper.ToDto<CartItem, CartItemDto>(entity);
    
    public static IQueryable<CartItemDto> ProjectToDto(IQueryable<CartItem> query) =>
        EntityMapper.ProjectToDto<CartItem, CartItemDto>(query);
}