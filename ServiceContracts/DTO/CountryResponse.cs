using Entities;

namespace ServiceContracts.DTO;

/// <summary>
///     DTO class used as return type for country
/// </summary>
public class CountryResponse
{
    public Guid CountryID { get; set; }
    public string? CountryName { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj.GetType() != typeof(CountryResponse)) return false;

        var countryToCompare = (CountryResponse)obj;

        return CountryID == countryToCompare.CountryID && CountryName == countryToCompare.CountryName;
    }
}

public static class CountryExtensions
{
    public static CountryResponse ToCountryResponse(this Country country)
    {
        return new CountryResponse
        {
            CountryID = country.CountryID,
            CountryName = country.CountryName
        };
    }
}