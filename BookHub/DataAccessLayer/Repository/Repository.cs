using System.Linq.Expressions;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly BookHubDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(BookHubDbContext context)
    {
        Context = context;
        DbSet = Context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);
    public void Add(T entity) => DbSet.Add(entity);

    public async Task<List<T>> GetAllAsync() => await DbSet.ToListAsync();
    public IQueryable<T> Query() => DbSet.AsQueryable();
    public void AddRange(IEnumerable<T> entities)
    {
        DbSet.AddRange(entities);
    }

    public void Update(T entity) => DbSet.Update(entity);
    public void Remove(T entity)
    {
        entity.IsRemoved = true;
        DbSet.Update(entity);
    }
}