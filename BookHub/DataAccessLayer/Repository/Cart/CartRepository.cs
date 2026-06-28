using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.Cart;

public class CartRepository : Repository<Models.Cart>, ICartRepository
{
    public CartRepository(BookHubDbContext context) : base(context)
    {
    }
    public new async Task<Models.Cart?> GetByIdAsync(int id) => await Context.Carts.AsNoTracking().FirstAsync(c => c.Id == id);

    public Task<Models.Cart?> GetCartByUserIdAsync(int userId) =>
        Context.Carts.AsNoTracking().Include(c => c.CartItems).ThenInclude(i => i.Book).Include(c => c.AppliedCoupon).ThenInclude(i => i.GiftCard).FirstOrDefaultAsync(c => c.UserId == userId);
}