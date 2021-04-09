namespace AirlyAnalyzer.Models
{
  using Microsoft.EntityFrameworkCore;
  using System;

  [Owned]
  public class Address
  {
    public string Country { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string Number { get; set; }

    public override string ToString()
    {
      string addressString = "";

      if (!String.IsNullOrWhiteSpace(this.Street))
      {
        addressString += this.Street;
      }

      if (!String.IsNullOrWhiteSpace(this.Number) && addressString != "")
      {
        if (this.Number != "")
        {
          addressString += " ";
        }

        addressString += this.Number;
      }

      if (!String.IsNullOrWhiteSpace(this.City))
      {
        if (addressString != "" && this.City != "")
        {
          addressString += ", ";
        }

        addressString += this.City;
      }

      if (!String.IsNullOrWhiteSpace(this.Country))
      {
        if (addressString != "" && this.Country != "")
        {
          addressString += ", ";
        }

        addressString += this.Country;
      }

      return addressString;
    }
  }
}
