namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  public interface IRepository<TEntity> where TEntity : class
  {
    public Task AddAsync(List<TEntity> entities);
    public void Delete(Expression<Func<TEntity, bool>> wherePredicate);

    public Task<List<TEntity>> Get(
        Expression<Func<TEntity, bool>> wherePredicate = null);

    public Task<List<T>> GetParameters<T>(
        Expression<Func<TEntity, T>> selectPredicate,
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByMethod = null,
        bool isDistinct = false);
  }
}
