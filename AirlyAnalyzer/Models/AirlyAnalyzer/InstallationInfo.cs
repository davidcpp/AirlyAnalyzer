using System;
using AirlyAnalyzer.Models;

namespace AirlyAnalyzer
{
  public class InstallationInfo
  {
    private DateTime requestDate;

    public short InstallationId { get; set; }

    public Address Address { get; set; }

    public DateTime RequestDate
    {
      get => requestDate.ToLocalTime();
      set => requestDate = value.ToUniversalTime();
    }
  }
}