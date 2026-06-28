namespace DataAccessLayer.Repository.Cart;

public interface ICartRepository : IRepository<Models.Cart>
{
    Task<Models.Cart?> GetCartByUserIdAsync(int userId);
}