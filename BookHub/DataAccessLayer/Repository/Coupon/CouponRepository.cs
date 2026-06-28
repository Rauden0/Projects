using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.Coupon;

public class CouponRepository : Repository<Models.Coupon>, ICouponRepository
{
    private readonly BookHubDbContext _db;
    public CouponRepository(BookHubDbContext db) : base(db) => _db = db;

    public new async Task<Models.Coupon?> GetByIdAsync(int id)
    {
        return await _db.Coupons.Include(c => c.GiftCard).FirstOrDefaultAsync(c => c.Id == id);
    }

    
}