using BusinessLayer.Dto.Wishlist;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IWishListService
{
    Task<Result<WishListDto>> GetWishList(int id);
    Task<Result<List<WishListDto>>> GetWishListsByUserId(int userId);
    Task<Result<List<WishListDto>>> GetWishLists(ODataQueryOptions<WishListDto>? options);
    Task<Result<WishListDto>> AddWishList(CreateWithListDto createWithListDto);
    Task<Result<WishListDto>> UpdateWishList(int id, string name);
    Task DeleteWishList(int id);
    Task<Result<WishListDto>> AddBookToWishList(int wishlistId, int bookId);
    Task RemoveBookFromWishList(int wishlistId, int bookId);
    
    Task<Result<List<WishListItemDto>>> GetWishlistItemsInWishList(int wishlistId);
}