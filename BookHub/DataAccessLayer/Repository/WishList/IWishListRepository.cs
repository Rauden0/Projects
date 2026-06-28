namespace DataAccessLayer.Repository.WishList;

public interface IWishListRepository : IRepository<Models.WishList>
{
    Task<List<Models.WishList>> GetWishListsByUserIdAsync(int userId);
}