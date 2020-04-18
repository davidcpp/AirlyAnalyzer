using System.Collections.Generic;

namespace AirlyAnalyzer.Models
{
  public class Measurements
  {
    public AveragedValues Current { get; set; }
    public List<AveragedValues> History { get; set; }
    public List<AveragedValues> Forecast { get; set; }
  }
}
