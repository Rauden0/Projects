namespace DataAccessLayer.Repository.Genre;

public interface IGenreRepository : IRepository<Models.Genre>
{
    Task<Models.Genre?> GetGenreByIdAsync(int id);
}