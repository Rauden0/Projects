namespace DataAccessLayer.Repository.Author;

public interface IAuthorRepository : IRepository<Models.Author>
{
    Task<Models.Author?> GetAuthorByIdAsync(int id);
}