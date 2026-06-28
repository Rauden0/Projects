namespace DataAccessLayer.Repository.Publisher;

public interface IPublisherRepository : IRepository<Models.Publisher>
{
    Task<Models.Publisher?> GetByIdAsync(int id);
}