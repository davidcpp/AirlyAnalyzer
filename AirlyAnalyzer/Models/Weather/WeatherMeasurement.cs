namespace AirlyAnalyzer.Models
{
  public class WeatherMeasurement
  {
    public int InstallationId { get; set; }

    public byte Month { get; set; }

    public byte Day { get; set; }

    public byte Hour { get; set; }

    // in Okta
    public byte CloudCover { get; set; }

    public byte Humidity { get; set; }

    public short Pressure { get; set; }

    // In milimeters
    public float Rain6h { get; set; }

    // In Celsius degrees
    public float Temperature { get; set; }

    // In meters
    public int Visibility { get; set; }

    // In m/s
    public float WindSpeed { get; set; }

    // In degrees
    public short WindDirection { get; set; }

    // In m/s
    public float WindGust { get; set; }

    public float AirlyCaqi { get; set; }
  }
}
