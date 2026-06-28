using BusinessLayer.Dto.Cart;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class CartMapper
{
    public static CartDto ToDto(Cart entity) =>
        EntityMapper.ToDto<Cart, CartDto>(entity);
    
    public static IQueryable<CartDto> ProjectToDto(IQueryable<Cart> query) =>
        EntityMapper.ProjectToDto<Cart, CartDto>(query);
}