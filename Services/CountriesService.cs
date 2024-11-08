using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    //private readonly List<Country> _countries;
    private readonly ICountriesRepository _countriesRepository;

    public CountriesService(ICountriesRepository countriesRepository) //,bool initialize = false
    {
        _countriesRepository = countriesRepository;
    }

    public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
    {
        //Validations
        if (countryAddRequest == null) throw new ArgumentNullException(nameof(countryAddRequest));

        if (countryAddRequest.CountryName == null) throw new ArgumentException(nameof(countryAddRequest.CountryName));

        if (await _countriesRepository.GetCountryByCountryName(countryAddRequest.CountryName) != null)
            throw new ArgumentException(nameof(countryAddRequest.CountryName));

        var country = countryAddRequest.ToCountry();
        country.CountryID = Guid.NewGuid();

        await _countriesRepository.AddCountry(country);

        return country.ToCountryResponse();
    }

    public async Task<List<CountryResponse>> GetAllCountries()
    {
        return (await _countriesRepository.GetAllCountries()).Select(c => c.ToCountryResponse()).ToList();
    }

    public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
    {
        if (countryID is null)
            return null;

        var country = await _countriesRepository.GetCountryByCountryID(countryID.Value);
        return country?.ToCountryResponse();
    }
}