namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  public interface IRepository<TEntity> where TEntity : class
  {
    Task AddAsync(TEntity entity);
    Task AddListAsync(List<TEntity> entities);
    bool Contains(TEntity entity);
    Task Delete(params object[] id);
    void Delete(Expression<Func<TEntity, bool>> wherePredicate);
    void Update(TEntity entity);

    Task<TEntity> GetById(params object[] id);

    Task<List<TEntity>> Get(
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByMethod = null,
        int count = 0);

    Task<List<T>> GetParameters<T>(
        Expression<Func<TEntity, T>> selectPredicate,
        Expression<Func<TEntity, bool>> wherePredicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByMethod = null,
        bool isDistinct = false);
  }
}
