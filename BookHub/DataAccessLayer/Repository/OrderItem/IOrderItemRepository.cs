namespace DataAccessLayer.Repository.OrderItem;

public interface IOrderItemRepository : IRepository<Models.OrderItem>
{
    Task<Models.OrderItem?> GetOrderItemByOrderIdAndBookIdAsync(int orderId, int bookId);
    
    Task<List<Models.OrderItem>> GetOrderItemsByBookIdAsync(int bookId);
    
    Task<List<Models.OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
}