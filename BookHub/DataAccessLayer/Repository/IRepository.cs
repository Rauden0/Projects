namespace DataAccessLayer.Repository;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    void Update(T entity);
    void Remove(T entity);
    
    void Add(T entity);

    Task<List<T>> GetAllAsync();

    IQueryable<T> Query();
    
    void AddRange(IEnumerable<T> entities);

}