using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.WishList;

public class WishListRepository : Repository<Models.WishList>, IWishListRepository
{
    public WishListRepository(BookHubDbContext context) : base(context)
    {
    }

    public Task<List<Models.WishList>> GetWishListsByUserIdAsync(int userId) =>
        Context.Wishlists.Where(w => w.UserId == userId).Include(w => w.WishlistItems).ToListAsync();
}