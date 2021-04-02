namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  public interface IRepository<TEntity> where TEntity : class
  {
    Task AddAsync(List<TEntity> entities);
    void Delete(Expression<Func<TEntity, bool>> wherePredicate);

    Task<TEntity> GetById(object id);

    Task<List<TEntity>> Get(
        Expression<Func<TEntity, bool>> wherePredicate = null);

    Task<List<T>> GetParameters<T>(
        Expression<Func<TEntity, T>> selectPredicate,
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByMethod = null,
        bool isDistinct = false);
  }
}
