namespace AirlyAnalyzer.Data
{
  using Microsoft.EntityFrameworkCore;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  public class BasicRepository<TEntity> : IRepository<TEntity> where TEntity : class
  {
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly AirlyContext _context;

    public BasicRepository(AirlyContext context)
    {
      _context = context;
      _dbSet = context.Set<TEntity>();
    }

    public async Task AddListAsync(List<TEntity> entities)
    {
      for (int i = 0; i < entities.Count;)
      {
        if (_dbSet.Contains(entities[i]))
        {
          entities.RemoveAt(i);
          continue;
        }
        i++;
      }

      await _dbSet.AddRangeAsync(entities);
    }

    public void Delete(Expression<Func<TEntity, bool>> wherePredicate)
    {
      _dbSet.RemoveRange(_dbSet.Where(wherePredicate));
    }

    public async Task<TEntity> GetById(object id)
    {
      return await _dbSet.FindAsync(id);
    }

    public Task<List<TEntity>> Get(
        Expression<Func<TEntity, bool>> wherePredicate = null)
    {
      IQueryable<TEntity> query = _dbSet;

      if (wherePredicate != null)
      {
        query = query.Where(wherePredicate);
      }

      return query.ToListAsync();
    }

    public Task<List<T>> GetParameters<T>(
        Expression<Func<TEntity, T>> selectPredicate,
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByMethod = null,
        bool isDistinct = false)
    {
      IQueryable<TEntity> query = _dbSet;

      if (wherePredicate != null)
      {
        query = query.Where(wherePredicate);
      }

      IQueryable<T> resultQuery;
      if (selectPredicate != null)
      {
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
