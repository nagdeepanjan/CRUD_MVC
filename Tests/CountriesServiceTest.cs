using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace Tests;

public class CountriesServiceTest
{
    private readonly ICountriesService _countriesService;

    public CountriesServiceTest()
    {
        _countriesService = new CountriesService();
    }

    #region GetAllCountries

    //List of countries should be empty by default
    [Fact]
    public void GetAllCountries_EmptyList()
    {
        //Act
        var countries = _countriesService.GetAllCountries();

        //Assert
        Assert.Empty(countries);
    }


    [Fact]
    public void GetAllCountries_AddFewCountries()
    {
        //Arrange
        var country_request_list = new List<CountryAddRequest>
        {
            new() { CountryName = "India" },
            new() { CountryName = "USA" },
            new() { CountryName = "UK" }
        };

        //Act
        var countries_list_from_add_country = new List<CountryResponse>();
        foreach (var country_request in country_request_list)
            countries_list_from_add_country.Add(_countriesService.AddCountry(country_request));
        var actualCountryResponseList = _countriesService.GetAllCountries();


        //Assert
        foreach (var expected_country in countries_list_from_add_country)
            Assert.Contains(expected_country, actualCountryResponseList);
    }

    #endregion

    #region AddCountry

    //When CountryAddRequest is null, then throw ArgumentNullException
    [Fact]
    public void AddCountry_NullCountryAddRequest()
    {
        //Arrange
        CountryAddRequest? request = null;


        //Act + Assert
        Assert.Throws<ArgumentNullException>(() => _countriesService.AddCountry(request));
    }

    //When CountryName is null, then throw ArgumentException
    [Fact]
    public void AddCountry_CountryNameisNull()
    {
        //Arrange
        var request = new CountryAddRequest { CountryName = null };


        //Assert
        Assert.Throws<ArgumentException>(() =>
        {
            //Act
            _countriesService.AddCountry(request);
        });
    }

    //When CountryName is duplicated, then throw ArgumentException
    [Fact]
    public void AddCountry_DuplicateCountryName()
    {
        //Arrange
        var request = new CountryAddRequest { CountryName = "xyz" };
        var request2 = new CountryAddRequest { CountryName = "xyz" };


        //Assert
        Assert.Throws<ArgumentException>(() =>
        {
            //Act
            _countriesService.AddCountry(request);
            _countriesService.AddCountry(request2);
        });
    }

    //When CountryName is valid, then it should insert(add) the country to list of countries & return CountryResponse with CountryID and Country
    [Fact]
    public void AddCountry_ProperCountryDetails()
    {
        //Arrange
        var request = new CountryAddRequest { CountryName = "China" };

        //Act
        var countryResponse = _countriesService.AddCountry(request);
        var countries_from_getAllCountries = _countriesService.GetAllCountries();

        //Assert
        Assert.True(countryResponse.CountryID != Guid.Empty);
        Assert.Contains(countryResponse, countries_from_getAllCountries);
    }

    #endregion
}