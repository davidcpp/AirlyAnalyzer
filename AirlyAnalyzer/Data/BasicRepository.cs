namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;

  public class BasicRepository<TEntity> : IRepository<TEntity> where TEntity : class
  {
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly DbContext _context;

    public BasicRepository(DbContext context)
    {
      _context = context;
      _dbSet = context.Set<TEntity>();
    }

    public virtual async Task AddAsync(TEntity entity)
    {
      if (!_dbSet.Contains(entity))
      {
        await _dbSet.AddAsync(entity);
      }
    }

    public virtual async Task AddListAsync(List<TEntity> entities)
    {
      await _dbSet.AddRangeAsync(entities);
    }

    public virtual bool Contains(TEntity entity)
    {
      return _dbSet.Contains(entity);
    }

    public virtual async Task Delete(params object[] id)
    {
      _dbSet.Remove(await _dbSet.FindAsync(id));
    }

    public virtual void Delete(Expression<Func<TEntity, bool>> wherePredicate)
    {
      _dbSet.RemoveRange(_dbSet.Where(wherePredicate));
    }

    public virtual void Update(TEntity entity)
    {
      _dbSet.Update(entity);
    }

    public virtual async Task<TEntity> GetById(params object[] id)
    {
      return await _dbSet.FindAsync(id);
    }

    public virtual Task<List<TEntity>> Get(
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByMethod = null,
        int count = 0)
    {
      IQueryable<TEntity> query = _dbSet;

      if (wherePredicate != null)
      {
        query = query.Where(wherePredicate);
      }

      if (orderByMethod != null)
      {
        query = orderByMethod(query);
      }

      if (count > 0)
      {
        query = query.Take(count);
      }

      return query.ToListAsync();
    }

    public virtual Task<List<T>> GetParameters<T>(
        Expression<Func<TEntity, T>> selectPredicate,
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByMethod = null,
        bool isDistinct = false)
    {
      IQueryable<TEntity> query = _dbSet;
      IQueryable<T> resultQuery;

      if (selectPredicate != null)
      {
        if (wherePredicate != null)
        {
          query = query.Where(wherePredicate);
        }

        resultQuery = query.Select(selectPredicate);

        if (isDistinct)
        {
          resultQuery = resultQuery.Distinct();
        }

        if (orderByMethod != null)
        {
          resultQuery = orderByMethod(resultQuery);
        }

        return resultQuery.ToListAsync();
      }
      else
      {
        return new Task<List<T>>(() => new List<T>());
      }
    }
  }
}
