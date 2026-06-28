using BusinessLayer.Dto.Wishlist;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

public class WishListService : IWishListService
{
    private readonly IUnitOfWork _uow;

    public WishListService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<WishListDto>> GetWishList(int id)
    {
        var wishlist = await _uow.WishLists.GetByIdAsync(id);
        return wishlist is not null
            ? WishListMapper.ToDto(wishlist)
            : new Result<WishListDto>(new Exception($"Wishlist {id} not found"));
    }
    
    public async Task<Result<List<WishListDto>>> GetWishListsByUserId(int userId)
    {
        var wishlist = await _uow.WishLists.GetWishListsByUserIdAsync(userId);
        return wishlist.Select(wishlistDto => WishListMapper.ToDto(wishlistDto)).ToList();
    }

    public async Task<Result<List<WishListDto>>> GetWishLists(ODataQueryOptions<WishListDto>? options)
    {
        return await WishListMapper.ProjectToDto(_uow.WishLists.Query()).ApplyIfNotNull(options).ToListAsync();
    }

    public async Task<Result<WishListDto>> AddWishList(CreateWithListDto createWithListDto)
    {
        var wishlist = new WishList
        {
            UserId = createWithListDto.UserId,
            Name = createWithListDto.Name
        };

        _uow.WishLists.Add(wishlist);
        await _uow.SaveChangesAsync();
        return WishListMapper.ToDto(wishlist);
    }

    public async Task<Result<WishListDto>> UpdateWishList(int id, string name)
    {
        var wishlist = await _uow.WishLists.GetByIdAsync(id);
        if (wishlist is null)
        {
            return new Result<WishListDto>(new Exception($"Wishlist {id} not found"));
        }

        wishlist.Name = name;

        _uow.WishLists.Update(wishlist);
        await _uow.SaveChangesAsync();
        return WishListMapper.ToDto(wishlist);
    }

    public async Task DeleteWishList(int id)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var wishlist = await _uow.WishLists.GetByIdAsync(id);
            if (wishlist is null)
            {
                return ;
            }
            foreach (var item in _uow.WishlistItems.Query().Where(w => w.WishlistId == id))
            {
                _uow.WishlistItems.Remove(item);
            }
            _uow.WishLists.Remove(wishlist);
            await _uow.SaveChangesAsync();
        });
    }

    public async Task<Result<WishListDto>> AddBookToWishList(int wishlistId, int bookId)
    {
        var wishlist = await _uow.WishLists.GetByIdAsync(wishlistId);
        if (wishlist is null)
        {
            return new Result<WishListDto>(new Exception($"Wishlist {wishlistId} not found"));
        }
        
        var existingWishListItem = await _uow.WishlistItems.GetWishListItemByWishListIdAndBookIdAsync(wishlistId, bookId);
        if (existingWishListItem is null)
        {
            var wishListItem = new WishlistItem
            {
                WishlistId = wishlistId, 
                BookId = bookId
            };
            _uow.WishlistItems.Add(wishListItem);
            await _uow.SaveChangesAsync();
        }
        return WishListMapper.ToDto(wishlist);
    }

    public async Task RemoveBookFromWishList(int wishlistId, int bookId)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var wishListItem = await _uow.WishlistItems.GetWishListItemByWishListIdAndBookIdAsync(wishlistId, bookId);
            if (wishListItem != null)
                _uow.WishlistItems.Remove(wishListItem);
            await _uow.SaveChangesAsync();
        });
    }
    
    public async Task<Result<List<WishListItemDto>>> GetWishlistItemsInWishList(int wishlistId)
    {
        if (await _uow.WishLists.GetByIdAsync(wishlistId) is null)
        {
            return new Result<List<WishListItemDto>>(new Exception($"Wishlist {wishlistId} not found"));
        }
        var wishListItems = await _uow.WishlistItems.GetWishListItemsByWishListIdAsync(wishlistId);
        return WishListItemMapper.ProjectToDto(wishListItems.AsQueryable()).ToList();
    }

}
