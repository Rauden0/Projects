using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.CartItem;

public class CartItemRepository : Repository<Models.CartItem>, ICartItemRepository
{
    public CartItemRepository(BookHubDbContext context) : base(context)
    {
    }

    public Task<Models.CartItem?> GetCartItemByCartIdAndBookIdAsync(int cartId, int bookId) =>
        Context.CartItems.AsNoTracking().FirstOrDefaultAsync(x => x.BookId == bookId && x.CartId == cartId);

    public Task<List<Models.CartItem>> GetCartItemsByCartIdAsync(int cartId) =>
        Context.CartItems.AsNoTracking().Where(c => c.CartId == cartId).ToListAsync();
}