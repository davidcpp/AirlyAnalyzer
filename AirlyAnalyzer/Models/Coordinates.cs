namespace AirlyAnalyzer.Models
{
  using Microsoft.EntityFrameworkCore;

  [Owned]
  public class Coordinates
  {
    public float Latitude { get; set; }

    public float Longitude { get; set; }
  }
}