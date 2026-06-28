using BusinessLayer.Dto.Wishlist;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class WishListItemMapper
{
    public static WishListItemDto ToDto(WishlistItem entity) =>
        EntityMapper.ToDto<WishlistItem, WishListItemDto>(entity);
    
    public static IQueryable<WishListItemDto> ProjectToDto(IQueryable<WishlistItem> query) =>
        EntityMapper.ProjectToDto<WishlistItem, WishListItemDto>(query);
}