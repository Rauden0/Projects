using System.Linq.Expressions;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.Book;

public class BookRepository : Repository<Models.Book>, IBookRepository
{
    public BookRepository(BookHubDbContext context) : base(context)
    {
    }

    public Task<List<Models.Book>> GetBooksByFilter(Expression<Func<Models.Book, bool>> predicate) =>
        Context.Books.Where(predicate).Include(b => b.Reviews).Include(b => b.Authors).Include(b => b.Genres).ToListAsync();

    public Task<Models.Book?> GetBookByIdwithAuthorAndGenre(int id) =>
        Context.Books.Include(b => b.Authors).Include(b => b.Genres).FirstOrDefaultAsync(b => b.Id == id);

    public Task<List<Models.Author>> GetAuthorsByListIds(List<int> ids) =>
        Context.Authors.Where(a => ids.Contains(a.Id)).ToListAsync();

    public Task<List<Models.Genre>> GetGenresByListIds(List<int> ids) =>
        Context.Genres.Where(g => ids.Contains(g.Id)).ToListAsync();
    
    
}