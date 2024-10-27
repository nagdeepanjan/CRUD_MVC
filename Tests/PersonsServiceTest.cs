using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions;

namespace Tests;

public class PersonsServiceTest
{
    private readonly ICountriesService _countriesService;
    private readonly IPersonsService _personsService;
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _personsService = new PersonsService();
        _countriesService = new CountriesService();
        _testOutputHelper = testOutputHelper;
    }

    #region GetAllPersons

    //The GetAllPersons() should return an empty list by default
    [Fact]
    public void GetAllPersons_EmptyList()
    {
        //Arrange

        //Act
        List<PersonResponse> persons = _personsService.GetAllPersons();

        //Assert
        Assert.Empty(persons);
    }

    //Add a few persons and then GetAllPersons() should return the list of all persons
    [Fact]
    public void GetAllPersons_AddFewPersons()
    {
        //Arrange
        CountryAddRequest country_request_1 = new CountryAddRequest { CountryName = "Japan" };
        CountryAddRequest country_request_2 = new CountryAddRequest { CountryName = "USA" };
        CountryAddRequest country_request_3 = new CountryAddRequest { CountryName = "India" };

        CountryResponse c1 = _countriesService.AddCountry(country_request_1);
        CountryResponse c2 = _countriesService.AddCountry(country_request_2);
        CountryResponse c3 = _countriesService.AddCountry(country_request_3);

        PersonAddRequest person_request_1 = new PersonAddRequest
        {
            PersonName = "Person 1",
            Email = "p1@gmail.com",
            Address = "Eugene",
            Gender = GenderOptions.Female,
            CountryID = c1.CountryID,
            DateOfBirth = DateTime.Parse("2020-01-01"),
            ReceiveNewsLetters = true
        };
        PersonAddRequest person_request_2 = new PersonAddRequest
        {
            PersonName = "Person 2",
            Email = "p2@gmail.com",
            Address = "Gainesville",
            Gender = GenderOptions.Male,
            CountryID = c2.CountryID,
            DateOfBirth = DateTime.Parse("2020-02-02"),
            ReceiveNewsLetters = true
        };
        PersonAddRequest person_request_3 = new PersonAddRequest
        {
            PersonName = "Person 3",
            Email = "p3@gmail.com",
            Address = "Hartford",
            Gender = GenderOptions.Female,
            CountryID = c3.CountryID,
            DateOfBirth = DateTime.Parse("2020-03-03"),
            ReceiveNewsLetters = true
        };

        //Act
        List<PersonResponse> person_response_list_from_add = new();
        List<PersonAddRequest> person_requests = new() { person_request_1, person_request_2, person_request_3 };

        foreach (PersonAddRequest person_request in person_requests)
        {
            PersonResponse person_response = _personsService.AddPerson(person_request);
            person_response_list_from_add.Add(person_response);
        }

        //print person_response_list_from_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response in person_response_list_from_add)
            _testOutputHelper.WriteLine(person_response.ToString()); //Using dependency injection

        List<PersonResponse> persons_list_from_get = _personsService.GetAllPersons();
        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_get.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        //Assert
        foreach (PersonResponse person_response in person_response_list_from_add)
            Assert.Contains(person_response, persons_list_from_get);
    }

    #endregion

    #region GetPersonByPersonID

    [Fact]
    public void GetPersonByPersonID_NullPersonID()
    {
        //Arrange
        Guid? personID = null;

        //Act
        PersonResponse? personResponse_from_get = _personsService.GetPersonByPersonID(personID);

        //Assert
        Assert.Null(personResponse_from_get);
    }

    //If we supply a valid person id, it should return the person object
    [Fact]
    public void GetPersonByPersonID_ValidPersonID()
    {
        //Arrange
        CountryAddRequest countryAddRequest = new CountryAddRequest { CountryName = "Japan" };
        CountryResponse countryResponse = _countriesService.AddCountry(countryAddRequest);

        //Act
        PersonAddRequest? personAddRequest = new PersonAddRequest
        {
            PersonName = "Person name...",
            CountryID = countryResponse.CountryID,
            Email = "test@gmail.com",
            Address = "Eugene",
            DateOfBirth = DateTime.Parse("2020-01-01"),
            Gender = GenderOptions.Male,
            ReceiveNewsLetters = true
        };

        PersonResponse person_response_from_add = _personsService.AddPerson(personAddRequest);

        PersonResponse? person_response_from_get =
            _personsService.GetPersonByPersonID(person_response_from_add.PersonID);

        //Assert
        Assert.Equal(person_response_from_get, person_response_from_add);
    }

    #endregion

    #region AddPerson

    //When we supply null value as PersonAddRequest, it should throw ArgumentNullException

    [Fact]
    public void AddPerson_NullPerson()
    {
        //Arrange
        PersonAddRequest? personAddRequest = null;

        //Assert
        Assert.Throws<ArgumentNullException>(
            //Act
            () => { _personsService.AddPerson(personAddRequest); });
    }

    [Fact]
    public void AddPerson_PersonNameIsNull()
    {
        //Arrange
        PersonAddRequest? personAddRequest = new PersonAddRequest { PersonName = null };

        //Assert
        Assert.Throws<ArgumentException>(
            //Act
            () => { _personsService.AddPerson(personAddRequest); });
    }

    //When we supply proper person details, it should insert the person into the persons list; and it should return an object of PersonResponse, which includes with the newly generated person id
    [Fact]
    public void AddPerson_ProperPersonDetails()
    {
        //Arrange
        PersonAddRequest? personAddRequest = new PersonAddRequest()
        {
            PersonName = "Person name...",
            Email = "person@example.com",
            Address = "sample address",
            CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Male,
            DateOfBirth = DateTime.Parse("2000-01-01"),
            ReceiveNewsLetters = true
        };

        //Act
        PersonResponse person_response_from_add = _personsService.AddPerson(personAddRequest);

        List<PersonResponse> persons_list = _personsService.GetAllPersons();

        //Assert
        Assert.True(person_response_from_add.PersonID != Guid.Empty);

        Assert.Contains(person_response_from_add, persons_list);
    }

    #endregion
}