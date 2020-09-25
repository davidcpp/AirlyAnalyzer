namespace AirlyAnalyzer.Models
{
  using System;
  using System.Collections.Generic;

  public class AveragedValues
  {
    private DateTime fromDateTime;
    private DateTime tillDateTime;

    public AveragedValues()
    {
      Values = new List<Measurement>();
      Indexes = new List<Index>();
      Standards = new List<Standard>();
    }

    public DateTime FromDateTime
    {
      get => fromDateTime.ToLocalTime();
      set => fromDateTime = value;
    }

    public DateTime TillDateTime
    {
      get => tillDateTime.ToLocalTime();
      set => tillDateTime = value;
    }

    public List<Measurement> Values { get; set; }
    public List<Index> Indexes { get; set; }
    public List<Standard> Standards { get; set; }
  }
}
