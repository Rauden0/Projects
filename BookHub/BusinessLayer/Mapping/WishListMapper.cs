using BusinessLayer.Dto.Wishlist;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class WishListMapper
{
    public static WishListDto ToDto(WishList entity) =>
        EntityMapper.ToDto<WishList, WishListDto>(entity);

    public static IQueryable<WishListDto> ProjectToDto(IQueryable<WishList> query) =>
        EntityMapper.ProjectToDto<WishList, WishListDto>(query);
}