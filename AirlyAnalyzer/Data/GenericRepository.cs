namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;

  public class GenericRepository<TEntity> : IRepository<TEntity>, IDisposable
      where TEntity : AirQualityObject
  {
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly AirlyContext _context;

    private readonly DateTime _dateTimeMinValue = new DateTime(2000, 1, 1);

    private bool disposedValue;

    public GenericRepository(AirlyContext context)
    {
      _context = context;
      _dbSet = context.Set<TEntity>();
    }

    public async Task Add(List<TEntity> entities)
    {
      while (entities.Count > 0 && _dbSet.Contains(entities[0]))
      {
        entities.RemoveAt(0);
      }

      await _dbSet.AddRangeAsync(entities);
    }

    public void Delete(Expression<Func<TEntity, bool>> wherePredicate)
    {
      _dbSet.RemoveRange(_dbSet.Where(wherePredicate));
    }

    public IEnumerable<TEntity> Get(
        Expression<Func<TEntity, bool>> wherePredicate = null)
    {
      IQueryable<TEntity> query = _dbSet;

      if (wherePredicate != null)
      {
        query = query.Where(wherePredicate);
      }

      return query.ToList();
    }

    public DateTime GetLastDate(short installationId)
    {
      var lastDate = _dateTimeMinValue;

      var selectedDates = GetParameters<DateTime>(
          wherePredicate: m => m.InstallationId == installationId,
          selectPredicate: m => m.TillDateTime,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime));

      if (selectedDates.Any())
      {
        lastDate = selectedDates.First();
      }

      return lastDate;
    }

    public IEnumerable<T> GetParameters<T>(
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

        return resultQuery.ToList();
      }
      else
      {
        return new List<T>();
      }
    }

    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _context.Dispose();
        }
        disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
