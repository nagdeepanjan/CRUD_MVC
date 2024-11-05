using Entities.DB;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    //private readonly List<Country> _countries;
    private readonly PersonsDbContext _db;

    public CountriesService(PersonsDbContext personsDbContext) //,bool initialize = false
    {
        _db = personsDbContext;
    }

    public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
    {
        //Validations
        if (countryAddRequest == null) throw new ArgumentNullException(nameof(countryAddRequest));

        if (countryAddRequest.CountryName == null) throw new ArgumentException(nameof(countryAddRequest.CountryName));

        if (await _db.Countries.AnyAsync(c => (c.CountryName ?? string.Empty).ToLower() == countryAddRequest.CountryName.ToLower()))
            throw new ArgumentException(nameof(countryAddRequest.CountryName));

        var country = countryAddRequest.ToCountry();
        country.CountryID = Guid.NewGuid();

        await _db.Countries.AddAsync(country);
        await _db.SaveChangesAsync();

        return country.ToCountryResponse();
    }

    public async Task<List<CountryResponse>> GetAllCountries()
    {
        return await _db.Countries.Select(c => c.ToCountryResponse()).ToListAsync();
    }

    public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
    {
        if (countryID is null)
            return null;

        var country = await _db.Countries.FirstOrDefaultAsync(c => c.CountryID == countryID);
        return country?.ToCountryResponse();
    }
}