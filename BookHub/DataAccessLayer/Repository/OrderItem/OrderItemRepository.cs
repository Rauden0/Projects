using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.OrderItem;

public class OrderItemRepository : Repository<Models.OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(BookHubDbContext context) : base(context)
    {
    }

    public Task<Models.OrderItem?> GetOrderItemByOrderIdAndBookIdAsync(int orderId, int bookId) =>
        Context.OrderItems.Where(o => o.OrderId == orderId && o.BookId == bookId).FirstOrDefaultAsync();
    
    public Task<List<Models.OrderItem>> GetOrderItemsByBookIdAsync(int bookId) => 
        Context.OrderItems.Where(oi => oi.BookId == bookId).ToListAsync();
    
    public Task<List<Models.OrderItem>> GetOrderItemsByOrderIdAsync(int orderId) =>
        Context.OrderItems.Where(o => o.OrderId == orderId).ToListAsync();
}