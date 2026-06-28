using System.Linq.Expressions;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.Author;

public class AuthorRepository : Repository<Models.Author>, IAuthorRepository
{
    private readonly BookHubDbContext _db;
    public AuthorRepository(BookHubDbContext db) : base(db) => _db = db;
    public Task<Models.Author?> GetAuthorByIdAsync(int id) =>
        _db.Authors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
}