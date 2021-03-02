namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  public interface IRepository<TEntity> where TEntity : class
  {
    public Task Add(IEnumerable<TEntity> entities);
    public void Delete(Expression<Func<TEntity, bool>> wherePredicate);

    public IEnumerable<TEntity> Get(
        Expression<Func<TEntity, bool>> wherePredicate = null);

    public IEnumerable<T> GetParameters<T>(
        Expression<Func<TEntity, T>> selectPredicate,
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByMethod = null,
        bool isDistinct = false);
  }
}
