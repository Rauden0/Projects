using DataAccessLayer.Data;

namespace DataAccessLayer.Repository.GiftCard;

public class GiftCardRepository : Repository<Models.GiftCard>, IGiftCardRepository
{
    private readonly BookHubDbContext _db;
    public GiftCardRepository(BookHubDbContext db) : base(db) => _db = db;
    
}