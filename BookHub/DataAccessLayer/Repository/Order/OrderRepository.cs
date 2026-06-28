using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.Order;

public class OrderRepository : Repository<Models.Order>, IOrderRepository
{
    public OrderRepository(BookHubDbContext context) : base(context)
    {
    }

    public Task<List<Models.Order>> GetOrdersByUserIdAsync(int userId) =>
        Context.Orders.Where(o => o.UserId == userId).Include(o => o.OrderItems).ToListAsync();
}