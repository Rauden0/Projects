namespace DataAccessLayer.Repository.Order;

public interface IOrderRepository : IRepository<Models.Order>
{
    Task<List<Models.Order>> GetOrdersByUserIdAsync(int userId);
}