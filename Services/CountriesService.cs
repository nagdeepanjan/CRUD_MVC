using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    private readonly List<Country> _countries;

    public CountriesService()
    {
        _countries = new List<Country>();
    }

    public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
    {
        //Validations
        if (countryAddRequest == null) throw new ArgumentNullException(nameof(countryAddRequest));

        if (countryAddRequest.CountryName == null) throw new ArgumentException(nameof(countryAddRequest.CountryName));

        if (_countries.Any(c => c.CountryName.ToLower() == countryAddRequest.CountryName.ToLower()))
            throw new ArgumentException(nameof(countryAddRequest.CountryName));
        //if (_countries.Where(c => c.CountryName == countryAddRequest.CountryName).Count() > 0)
        //    throw new ArgumentException(nameof(countryAddRequest.CountryName));


        var country = countryAddRequest.ToCountry();
        country.CountryID = Guid.NewGuid();

        _countries.Add(country);

        return country.ToCountryResponse();
    }

    public List<CountryResponse> GetAllCountries()
    {
        return _countries.Select(c => c.ToCountryResponse()).ToList();
    }

    public CountryResponse? GetCountryByCountryID(Guid? countryID)
    {
        throw new NotImplementedException();
    }
}