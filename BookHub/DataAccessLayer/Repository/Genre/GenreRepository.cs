using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.Genre;

public class GenreRepository : Repository<Models.Genre>, IGenreRepository
{
    private readonly BookHubDbContext _db;
    public GenreRepository(BookHubDbContext db) : base(db) => _db = db;

    public Task<Models.Genre?> GetGenreByIdAsync(int id) =>
        _db.Genres.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
}