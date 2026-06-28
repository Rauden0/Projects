namespace DataAccessLayer.Repository.User;

public interface IUserRepository : IRepository<Models.User>
{
    Task<Models.User?> GetUserByEmailAsync(string email);
}