namespace DataAccessLayer.Repository.CartItem;

public interface ICartItemRepository : IRepository<Models.CartItem>
{
    Task<Models.CartItem?> GetCartItemByCartIdAndBookIdAsync(int cartId, int bookId);
    Task<List<Models.CartItem>> GetCartItemsByCartIdAsync(int cartId);
}