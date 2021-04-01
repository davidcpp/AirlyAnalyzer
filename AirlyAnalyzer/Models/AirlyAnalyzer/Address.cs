using Microsoft.EntityFrameworkCore;

namespace AirlyAnalyzer.Models
{
  [Owned]
  public class Address
  {
    public string Country { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string Number { get; set; }
  }
}
