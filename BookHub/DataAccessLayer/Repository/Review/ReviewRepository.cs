using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.Review;

public class ReviewRepository : Repository<Models.Review>, IReviewRepository
{

    public ReviewRepository(BookHubDbContext context) : base(context)
    {
    }
    public Task<List<Models.Review>> GetReviewsByBookIdAsync(int bookId) =>
        Context.Reviews.Where(r => r.BookId == bookId).ToListAsync();

}