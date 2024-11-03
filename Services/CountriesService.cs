using Entities.DB;
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
        //_countries = new List<Country>();
        //if (initialize)
        //    _countries.AddRange(new List<Country>
        //    {
        //        new() { CountryID = Guid.Parse("4DFDC6A6-FACB-4E5E-B49D-3F597B64B5BD"), CountryName = "USA" },
        //        new() { CountryID = Guid.Parse("E45DBE1B-678F-4523-A1BB-DF8D10D974E7"), CountryName = "Canada" },
        //        new() { CountryID = Guid.Parse("627A9ECE-B531-4C07-ACF5-9234DB388D59"), CountryName = "Mexico" },
        //        new() { CountryID = Guid.Parse("34AC70D4-E91B-4929-BF98-5B198E6FD798"), CountryName = "Brazil" },
        //        new() { CountryID = Guid.Parse("7B11EB3F-CF1A-4C5D-AC88-097A8A196DD5"), CountryName = "Japan" }
        //    });
    }

    public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
    {
        //Validations
        if (countryAddRequest == null) throw new ArgumentNullException(nameof(countryAddRequest));

        if (countryAddRequest.CountryName == null) throw new ArgumentException(nameof(countryAddRequest.CountryName));

        //if (_countries.Any(c => c.CountryName.ToLower() == countryAddRequest.CountryName.ToLower()))
        if (_db.Countries.Any(c => (c.CountryName ?? string.Empty).ToLower() == countryAddRequest.CountryName.ToLower()))
            throw new ArgumentException(nameof(countryAddRequest.CountryName));
        //if (_countries.Where(c => c.CountryName == countryAddRequest.CountryName).Count() > 0)
        //    throw new ArgumentException(nameof(countryAddRequest.CountryName));


        var country = countryAddRequest.ToCountry();
        country.CountryID = Guid.NewGuid();

        //_countries.Add(country);
        _db.Countries.Add(country);
        _db.SaveChanges();

        return country.ToCountryResponse();
    }

    public List<CountryResponse> GetAllCountries()
    {
        //return _countries.Select(c => c.ToCountryResponse()).ToList();
        return _db.Countries.Select(c => c.ToCountryResponse()).ToList();
    }

    public CountryResponse? GetCountryByCountryID(Guid? countryID)
    {
        if (countryID is null)
            return null;

        //return _countries.FirstOrDefault(c => c.CountryID == countryID)?.ToCountryResponse();

        return _db.Countries.FirstOrDefault(c => c.CountryID == countryID)?.ToCountryResponse();
    }
}