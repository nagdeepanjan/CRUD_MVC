using Entities;
using Entities.DB;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace Tests;

public class CountriesServiceTest
{
    private readonly ICountriesService _countriesService;

    public CountriesServiceTest()
    {
        //Mocking DbContext
        DbContextMock<ApplicationDbContext> dbContextMock = new(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
        ApplicationDbContext dbContext = dbContextMock.Object;

        //Mocking DBSet
        var countriesInitialData = new List<Country> { };
        dbContextMock.CreateDbSetMock(x => x.Countries, countriesInitialData);


        //var dbContext = new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options);
        _countriesService = new CountriesService(null);
    }

    #region GetCountryByCountryID

    [Fact]
    public async Task GetCountryByCountryID_NullCountryID()
    {
        //Arrange
        Guid? countryID = null;

        //Act
        CountryResponse? country_response_from_method = await _countriesService.GetCountryByCountryID(countryID);

        //Assert
        Assert.Null(country_response_from_method);
    }

    [Fact]
    public async Task GetCountryByCountryID_ValidCountryID()
    {
        //Arrange
        CountryAddRequest country_add_request = new CountryAddRequest { CountryName = "Japan" };
        CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

        //Act
        CountryResponse? country_response_from_get =
            await _countriesService.GetCountryByCountryID(country_response_from_add.CountryID);

        //Assert
        Assert.Equal(country_response_from_get, country_response_from_add);
    }

    #endregion

    #region GetAllCountries

    //List of countries should be empty by default
    [Fact]
    public async Task GetAllCountries_EmptyList()
    {
        //Act
        var countries = await _countriesService.GetAllCountries();

        //Assert
        Assert.Empty(countries);
    }


    [Fact]
    public async Task GetAllCountries_AddFewCountries()
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
            countries_list_from_add_country.Add(await _countriesService.AddCountry(country_request));
        var actualCountryResponseList = await _countriesService.GetAllCountries();


        //Assert
        foreach (var expected_country in countries_list_from_add_country)
            Assert.Contains(expected_country, actualCountryResponseList);
    }

    #endregion

    #region AddCountry

    //When CountryAddRequest is null, then throw ArgumentNullException
    [Fact]
    public async Task AddCountry_NullCountryAddRequest()
    {
        //Arrange
        CountryAddRequest? request = null;


        //Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _countriesService.AddCountry(request));
    }

    //When CountryName is null, then throw ArgumentException
    [Fact]
    public async Task AddCountry_CountryNameisNull()
    {
        //Arrange
        var request = new CountryAddRequest { CountryName = null };


        //Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            //Act
            await _countriesService.AddCountry(request);
        });
    }

    //When CountryName is duplicated, then throw ArgumentException
    [Fact]
    public async Task AddCountry_DuplicateCountryName()
    {
        //Arrange
        var request = new CountryAddRequest { CountryName = "xyz" };
        var request2 = new CountryAddRequest { CountryName = "xyz" };


        //Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            //Act
            await _countriesService.AddCountry(request);
            await _countriesService.AddCountry(request2);
        });
    }

    //When CountryName is valid, then it should insert(add) the country to list of countries & return CountryResponse with CountryID and Country
    [Fact]
    public async Task AddCountry_ProperCountryDetails()
    {
        //Arrange
        var request = new CountryAddRequest { CountryName = "China" };

        //Act
        var countryResponse = await _countriesService.AddCountry(request);
        var countries_from_getAllCountries = await _countriesService.GetAllCountries();

        //Assert
        Assert.True(countryResponse.CountryID != Guid.Empty);
        Assert.Contains(countryResponse, countries_from_getAllCountries);
    }

    #endregion
}