using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.WishListItem;

public class WishListItemRepository : Repository<Models.WishlistItem>, IWishListItemRepository
{
    public WishListItemRepository(BookHubDbContext context) : base(context)
    {
    }

    public Task<WishlistItem?> GetWishListItemByWishListIdAndBookIdAsync(int wishlistId, int bookId) =>
        Context.WishlistItems.Where(wi => wi.WishlistId == wishlistId && wi.BookId == bookId).FirstOrDefaultAsync();
    
    public Task<List<WishlistItem>> GetWishListItemsByWishListIdAsync(int wishlistId) =>
        Context.WishlistItems.Where(wi => wi.WishlistId == wishlistId).ToListAsync();
}