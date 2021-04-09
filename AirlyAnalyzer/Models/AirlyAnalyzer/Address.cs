﻿using Microsoft.EntityFrameworkCore;

namespace AirlyAnalyzer.Models
{
  [Owned]
  public class Address
  {
    public string Country { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string Number { get; set; }

    public override string ToString()
    {
      return (this.Street ?? "") + " "
          + (this.Number ?? "") + ", "
          + (this.City ?? "") + ", "
          + (this.Country ?? "");
    }
  }
}
