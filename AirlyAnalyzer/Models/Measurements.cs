namespace AirlyAnalyzer.Models
{
  using System.Collections.Generic;

  public class Measurements
  {
    public Measurements()
    {
      Current = new AveragedValues();
      Forecast = new List<AveragedValues>();
      History = new List<AveragedValues>();
    }

    public AveragedValues Current { get; set; }

    public List<AveragedValues> History { get; set; }

    public List<AveragedValues> Forecast { get; set; }
  }
}
