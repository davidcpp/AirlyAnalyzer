namespace AirlyAnalyzer.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using AirlyAnalyzer.Models;
  using Microsoft.EntityFrameworkCore;

  public class AirQualityRepository<TEntity> : BasicRepository<TEntity>
      where TEntity : AirQualityObject
  {
    protected readonly DateTime _dateTimeMinValue = new DateTime(2000, 1, 1);

    public AirQualityRepository(AirlyContext context) : base(context)
    {
    }

    public override Task AddListAsync(List<TEntity> entities)
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

      return _dbSet.AddRangeAsync(entities);
    }

    public virtual async Task<List<TEntity>> GetLastElements(
        short numberOfElements,
        Expression<Func<TEntity, bool>> wherePredicate = null)
    {
      var lastElements = await Get(
          wherePredicate,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime),
          numberOfElements);

      return (from x in lastElements
             orderby x.TillDateTime
             select x).ToList();
    }

    public virtual async Task<DateTime> GetLastDate(
        short installationId,
        AirQualityDataSource source = AirQualityDataSource.Airly)
    {
      var lastDate = _dateTimeMinValue;

      var selectedDates = await GetParameters(
          wherePredicate: x => x.InstallationId == installationId
                            && x.Source == source,
          selectPredicate: x => x.TillDateTime,
          orderByMethod: q => q.OrderByDescending(dateTime => dateTime),
          count: 1);

      if (selectedDates.Count > 0)
      {
        lastDate = selectedDates[0];
      }

      return lastDate;
    }
  }
}
