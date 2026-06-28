namespace DataAccessLayer.Repository.Review;

public interface IReviewRepository : IRepository<Models.Review>
{
    Task<List<Models.Review>> GetReviewsByBookIdAsync(int bookId);


}