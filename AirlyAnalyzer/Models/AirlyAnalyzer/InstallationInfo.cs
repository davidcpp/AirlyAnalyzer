using System;
using AirlyAnalyzer.Models;

namespace AirlyAnalyzer
{
  public class InstallationInfo
  {
    private DateTime updateDate;

    public short InstallationId { get; set; }

    public Address Address { get; set; }

    public Coordinates Location { get; set; }

    public DateTime UpdateDate
    {
      get => updateDate.ToLocalTime();
      set => updateDate = value.ToUniversalTime();
    }
  }
}