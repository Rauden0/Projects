namespace DataAccessLayer.Repository.WishListItem;

public interface IWishListItemRepository : IRepository<Models.WishlistItem>
{
    Task<Models.WishlistItem?> GetWishListItemByWishListIdAndBookIdAsync(int wishId, int bookId);
    
    Task<List<Models.WishlistItem>> GetWishListItemsByWishListIdAsync(int wishId);
}