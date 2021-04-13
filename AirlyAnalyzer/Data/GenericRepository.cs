namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;

  public class GenericRepository<TEntity> : IRepository<TEntity>
      where TEntity : AirQualityObject
  {
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly AirlyContext _context;

    protected readonly DateTime _dateTimeMinValue = new DateTime(2000, 1, 1);

    public GenericRepository(AirlyContext context)
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

    public virtual bool Contains(TEntity entity)
    {
      return _dbSet.Contains(entity);
    }

    public virtual async Task Delete(object id)
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

    public virtual async Task<TEntity> GetById(object id)
    {
      return await _dbSet.FindAsync(id);
    }

    public virtual Task<List<TEntity>> Get(
        Expression<Func<TEntity, bool>> wherePredicate = null)
    {
      IQueryable<TEntity> query = _dbSet;

      if (wherePredicate != null)
      {
        query = query.Where(wherePredicate);
      }

      return query.ToListAsync();
    }

    public virtual async Task<DateTime> GetLastDate(short installationId)
    {
      var lastDate = _dateTimeMinValue;

      var selectedDates = await GetParameters(
          wherePredicate: x => x.InstallationId == installationId,
          selectPredicate: x => x.TillDateTime,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime));

      if (selectedDates.Count > 0)
      {
        lastDate = selectedDates[0];
      }

      return lastDate;
    }

    public virtual Task<List<T>> GetParameters<T>(
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
