namespace AirlyAnalyzer.Client
{
  using System;
  using AirlyAnalyzer.Models;

  public static class InstallationDataConverter
  {
    public static InstallationInfo ConvertToInstallationInfo(
        this Installation installation, DateTime updateDate)
    {
      return new InstallationInfo()
      {
        InstallationId = (short)installation.Id,
        UpdateDate = updateDate,
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

