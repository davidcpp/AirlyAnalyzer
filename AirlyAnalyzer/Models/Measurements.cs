namespace AirlyAnalyzer.Models
{
  using System.Collections.Generic;

  public class Measurements
  {
    public AveragedValues Current { get; set; }
    public List<AveragedValues> History { get; set; }
    public List<AveragedValues> Forecast { get; set; }
  }
}
