using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository.Publisher;

public class PublisherRepository : Repository<Models.Publisher>, IPublisherRepository
{
    private readonly BookHubDbContext _db;
    public PublisherRepository(BookHubDbContext db) : base(db) => _db = db;

    public Task<Models.Publisher?> GetPublisherByIdAsync(int id) =>
        _db.Publishers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
}