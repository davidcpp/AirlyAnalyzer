namespace AirlyAnalyzer.Models
{
  public class Installation
  {
    public Installation()
    {
      Location = new Coordinates();
      Address = new InstallationAddress();
      Sponsor = new InstallationSponsor();
    }

    public int Id { get; set; }

    public Coordinates Location { get; set; }

    public int LocationId { get; set; }

    public InstallationAddress Address { get; set; }

    public float Elevation { get; set; }

    public bool Airly { get; set; }

    public InstallationSponsor Sponsor { get; set; }
  }
}