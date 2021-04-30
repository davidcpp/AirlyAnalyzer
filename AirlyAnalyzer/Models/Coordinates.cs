namespace AirlyAnalyzer.Models
{
  using Microsoft.EntityFrameworkCore;

  [Owned]
  public class Coordinates
  {
    public double Latitude { get; set; }

    public double Longitude { get; set; }
  }
}