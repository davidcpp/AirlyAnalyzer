namespace AirlyAnalyzer.Client
{
  using System;
  using AirlyAnalyzer.Models;

  public static class InstallationDataConverter
  {
    public static InstallationInfo ConvertToInstallationInfo(
        this Installation installation, DateTime requestDate)
    {
      return new InstallationInfo()
      {
        InstallationId = (short)installation.Id,
        RequestDate = requestDate,
        Address = new Address()
        {
          Country = installation.Address.Country ?? "",
          City = installation.Address.City ?? "",
          Street = installation.Address.Street ?? "",
          Number = installation.Address.Number ?? "",
        }
      };
    }
  }
}

