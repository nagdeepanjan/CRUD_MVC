using AutoFixture;
using Entities;
using Entities.DB;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions;

namespace Tests;

public class PersonsServiceTest
{
    private readonly ICountriesService _countriesService;
    private readonly IFixture _fixture;
    private readonly IPersonsService _personsService;
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();

        //Data for testing
        var countriesInitialData = new List<Country> { };
        var personsInitialData = new List<Person> { };


        //Mocking DbContext
        DbContextMock<ApplicationDbContext> dbContextMock = new(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
        ApplicationDbContext dbContext = dbContextMock.Object;

        //Mocking DBSet        
        dbContextMock.CreateDbSetMock(x => x.Countries, countriesInitialData);
        dbContextMock.CreateDbSetMock(x => x.Persons, personsInitialData);


        _countriesService = new CountriesService(dbContext);
        _personsService = new PersonsService(dbContext, _countriesService);

        _testOutputHelper = testOutputHelper;
    }

    #region GetSortedPersons

    [Fact]
    public async Task GetSortedPersons()
    {
        //Arrange
        CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
        CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();
        CountryAddRequest country_request_3 = _fixture.Create<CountryAddRequest>();

        CountryResponse c1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse c2 = await _countriesService.AddCountry(country_request_2);
        CountryResponse c3 = await _countriesService.AddCountry(country_request_3);

        PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email1@mail.com").With(p => p.PersonName, "Zilog").With(p => p.CountryID, c1.CountryID).Create();
        PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email2@mail.com").With(p => p.PersonName, "Apple").With(p => p.CountryID, c2.CountryID).Create();
        PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email3@mail.com").With(p => p.PersonName, "Google").With(p => p.CountryID, c3.CountryID).Create();


        List<PersonResponse> person_response_list_from_add = new();
        List<PersonAddRequest> person_requests = new() { person_request_1, person_request_2, person_request_3 };

        foreach (PersonAddRequest person_request in person_requests)
        {
            PersonResponse person_response = await _personsService.AddPerson(person_request);
            person_response_list_from_add.Add(person_response);
        }

        //Sorting by Name
        person_response_list_from_add = person_response_list_from_add.OrderBy(p => p.PersonName).ToList();

        List<PersonResponse> allPersons = await _personsService.GetAllPersons();

        //print person_response_list_from_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response in person_response_list_from_add)
            _testOutputHelper.WriteLine(person_response.ToString()); //Using dependency injection


        //Act
        List<PersonResponse> persons_list_from_sort =
            await _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.ASC);
        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_sort.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        //Assert
        for (int i = 0; i < person_response_list_from_add.Count; i++)
            Assert.Equal(person_response_list_from_add[i], persons_list_from_sort[i]);
    }

    #endregion

    #region UpdatePerson

    //When we supply null value as PersonUpdateRequest, it should throw ArgumentNullException
    [Fact]
    public async Task UpdatePerson_NullPerson()
    {
        //Arrange
        PersonUpdateRequest? personUpdateRequest = null;

        //Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            //Act
            await _personsService.UpdatePerson(personUpdateRequest);
        });
    }

    //When invalid person id is supplied, it should throw ArgumentException
    [Fact]
    public async Task UpdatePerson_InvalidPersonID()
    {
        //Arrange
        //PersonUpdateRequest? personUpdateRequest = new PersonUpdateRequest { PersonID = Guid.NewGuid() };
        PersonUpdateRequest? personUpdateRequest = _fixture.Create<PersonUpdateRequest>(); //.Create();

        //Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            //Act
            await _personsService.UpdatePerson(personUpdateRequest);
        });
    }

    //When personName is null, it should throw ArgumentException

    [Fact]
    public async Task UpdatePerson_PersonNameIsNull()
    {
        //Arrange
        //Create a country, a person using that country and then update the person with null person name
        CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
        CountryResponse countryResponse_from_add = await _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.PersonName, "Rahul").With(p => p.Email, "test@gmail.com").Create();


        PersonResponse personResponse_from_add = await _personsService.AddPerson(personAddRequest);

        PersonUpdateRequest personUpdateRequest = personResponse_from_add.ToPersonUpdateRequest();
        personUpdateRequest.PersonName = null;

        //Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            //Act
            await _personsService.UpdatePerson(personUpdateRequest);
        });
    }

    //Normal updating of person
    [Fact]
    public async Task UpdatePerson_ProperValues()
    {
        //Arrange
        //Create a country, a person using that country and then update the person with null person name
        CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
        CountryResponse countryResponse_from_add = await _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.PersonName, "Rahul").With(p => p.Email, "test@gmail.com").Create();


        PersonResponse personResponse_from_add = await _personsService.AddPerson(personAddRequest);

        PersonUpdateRequest personUpdateRequest = personResponse_from_add.ToPersonUpdateRequest();
        personUpdateRequest.PersonName = "P2";
        personUpdateRequest.Email = "p2@gmail.com";


        //Act
        PersonResponse personResponse_from_update = await _personsService.UpdatePerson(personUpdateRequest);


        PersonResponse personResponse_from_get = await _personsService.GetPersonByPersonID(personResponse_from_update.PersonID);


        //Assert
        Assert.Equal(personResponse_from_update, personResponse_from_get);
    }

    #endregion


    #region GetFilteredPersons

    //If searchBy is PersonName and searcgString is empty, it should return all persons
    [Fact]
    public async Task GetFilteredPersons_EmptySearchText()
    {
        //Arrange
        CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>(); //new CountryAddRequest { CountryName = "Japan" };
        CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>(); //new CountryAddRequest { CountryName = "USA" };
        CountryAddRequest country_request_3 = _fixture.Create<CountryAddRequest>(); //new CountryAddRequest { CountryName = "India" };

        CountryResponse c1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse c2 = await _countriesService.AddCountry(country_request_2);
        CountryResponse c3 = await _countriesService.AddCountry(country_request_3);


        PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email1@mail.com").Create();
        PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email2@mail.com").Create();
        PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email3@mail.com").Create();


        List<PersonResponse> person_response_list_from_add = new();
        List<PersonAddRequest> person_requests = new() { person_request_1, person_request_2, person_request_3 };

        foreach (PersonAddRequest person_request in person_requests)
        {
            PersonResponse person_response = await _personsService.AddPerson(person_request);
            person_response_list_from_add.Add(person_response);
        }

        //print person_response_list_from_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response in person_response_list_from_add)
            _testOutputHelper.WriteLine(person_response.ToString()); //Using dependency injection


        //Act
        List<PersonResponse> persons_list_from_search =
            await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");
        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_search.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        //Assert
        foreach (PersonResponse person_response in person_response_list_from_add)
            Assert.Contains(person_response, persons_list_from_search);
    }

    //First we will add a few people and then search based on person name with some search string.
    [Fact]
    public async Task GetFilteredPersons_SearchByPersonName()
    {
        //Arrange
        CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
        CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();
        CountryAddRequest country_request_3 = _fixture.Create<CountryAddRequest>();

        CountryResponse c1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse c2 = await _countriesService.AddCountry(country_request_2);
        CountryResponse c3 = await _countriesService.AddCountry(country_request_3);

        PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email1@mail.com").With(p => p.PersonName, "Rahman").With(p => p.CountryID, c1.CountryID).Create();
        PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email2@mail.com").With(p => p.PersonName, "Mary").With(p => p.CountryID, c2.CountryID).Create();
        PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email3@mail.com").With(p => p.PersonName, "Mani").With(p => p.CountryID, c3.CountryID).Create();


        List<PersonResponse> person_response_list_from_add = new();
        List<PersonAddRequest> person_requests = new() { person_request_1, person_request_2, person_request_3 };

        foreach (PersonAddRequest person_request in person_requests)
        {
            PersonResponse person_response = await _personsService.AddPerson(person_request);
            person_response_list_from_add.Add(person_response);
        }

        //print person_response_list_from_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response in person_response_list_from_add)
            _testOutputHelper.WriteLine(person_response.ToString()); //Using dependency injection


        //Act
        List<PersonResponse> persons_list_from_search =
            await _personsService.GetFilteredPersons(nameof(Person.PersonName), "ma");
        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_search.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        //Assert

        foreach (PersonResponse person_response in person_response_list_from_add)
            if (person_response.PersonName != null)
                if (person_response.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                    Assert.Contains(person_response, persons_list_from_search);
    }

    #endregion

    #region GetAllPersons

    //The GetAllPersons() should return an empty list by default
    [Fact]
    public async Task GetAllPersons_EmptyList()
    {
        //Arrange


        //Act
        List<PersonResponse> persons = await _personsService.GetAllPersons();

        //Assert
        Assert.Empty(persons);
    }

    //Add a few persons and then GetAllPersons() should return the list of all persons
    [Fact]
    public async Task GetAllPersons_AddFewPersons()
    {
        //Arrange
        CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>(); //new CountryAddRequest { CountryName = "Japan" };
        CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>(); //new CountryAddRequest { CountryName = "USA" };
        CountryAddRequest country_request_3 = _fixture.Create<CountryAddRequest>(); //new CountryAddRequest { CountryName = "India" };

        CountryResponse c1 = await _countriesService.AddCountry(country_request_1);
        CountryResponse c2 = await _countriesService.AddCountry(country_request_2);
        CountryResponse c3 = await _countriesService.AddCountry(country_request_3);

        //PersonAddRequest person_request_1 = new PersonAddRequest
        //{
        //    PersonName = "Person 1",
        //    Email = "p1@gmail.com",
        //    Address = "Eugene",
        //    Gender = GenderOptions.Female,
        //    CountryID = c1.CountryID,
        //    DateOfBirth = DateTime.Parse("2020-01-01"),
        //    ReceiveNewsLetters = true
        //};
        //PersonAddRequest person_request_2 = new PersonAddRequest
        //{
        //    PersonName = "Person 2",
        //    Email = "p2@gmail.com",
        //    Address = "Gainesville",
        //    Gender = GenderOptions.Male,
        //    CountryID = c2.CountryID,
        //    DateOfBirth = DateTime.Parse("2020-02-02"),
        //    ReceiveNewsLetters = true
        //};
        //PersonAddRequest person_request_3 = new PersonAddRequest
        //{
        //    PersonName = "Person 3",
        //    Email = "p3@gmail.com",
        //    Address = "Hartford",
        //    Gender = GenderOptions.Female,
        //    CountryID = c3.CountryID,
        //    DateOfBirth = DateTime.Parse("2020-03-03"),
        //    ReceiveNewsLetters = true
        //};

        PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email1@mail.com").Create();
        PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email2@mail.com").Create();
        PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>().With(p => p.Email, "email3@mail.com").Create();


        //Act
        List<PersonResponse> person_response_list_from_add = new();
        List<PersonAddRequest> person_requests = new() { person_request_1, person_request_2, person_request_3 };

        foreach (PersonAddRequest person_request in person_requests)
        {
            PersonResponse person_response = await _personsService.AddPerson(person_request);
            person_response_list_from_add.Add(person_response);
        }

        //print person_response_list_from_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response in person_response_list_from_add)
            _testOutputHelper.WriteLine(person_response.ToString()); //Using dependency injection

        List<PersonResponse> persons_list_from_get = await _personsService.GetAllPersons();
        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_get.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        //Assert
        foreach (PersonResponse person_response in person_response_list_from_add)
            Assert.Contains(person_response, persons_list_from_get);
    }

    #endregion

    #region GetPersonByPersonID

    [Fact]
    public async Task GetPersonByPersonID_NullPersonID()
    {
        //Arrange
        Guid? personID = null;


        //Act
        PersonResponse? personResponse_from_get = await _personsService.GetPersonByPersonID(personID);

        //Assert
        Assert.Null(personResponse_from_get);
    }

    //If we supply a valid person id, it should return the person object
    [Fact]
    public async Task GetPersonByPersonID_ValidPersonID()
    {
        //Arrange
        //CountryAddRequest countryAddRequest = new CountryAddRequest { CountryName = "Japan" };
        CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
        CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);


        //Act
        //PersonAddRequest? personAddRequest = new PersonAddRequest
        //{
        //    PersonName = "Person name...",
        //    CountryID = countryResponse.CountryID,
        //    Email = "test@gmail.com",
        //    Address = "Eugene",
        //    DateOfBirth = DateTime.Parse("2020-01-01"),
        //    Gender = GenderOptions.Male,
        //    ReceiveNewsLetters = true
        //};
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.Email, "test@test.com").Create();

        PersonResponse person_response_from_add = await _personsService.AddPerson(personAddRequest);

        PersonResponse? person_response_from_get =
            await _personsService.GetPersonByPersonID(person_response_from_add.PersonID);

        //Assert
        Assert.Equal(person_response_from_get, person_response_from_add);
    }

    #endregion

    #region AddPerson

    //When we supply null value as PersonAddRequest, it should throw ArgumentNullException

    [Fact]
    public async Task AddPerson_NullPerson()
    {
        //Arrange
        PersonAddRequest? personAddRequest = null;

        //Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            //Act
            async () => { await _personsService.AddPerson(personAddRequest); });
    }

    [Fact]
    public async Task AddPerson_PersonNameIsNull()
    {
        //Arrange with AutoFixture
        //PersonAddRequest? personAddRequest = new PersonAddRequest { PersonName = null };
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.PersonName, null as string).Create(); //null as string is important

        //Assert
        await Assert.ThrowsAsync<ArgumentException>(
            //Act
            async () => { await _personsService.AddPerson(personAddRequest); });
    }

    //When we supply proper person details, it should insert the person into the persons list; and it should return an object of PersonResponse, which includes with the newly generated person id
    [Fact]
    public async Task AddPerson_ProperPersonDetails()
    {
        //Arrange
        //PersonAddRequest? personAddRequest = new PersonAddRequest()
        //{
        //    PersonName = "Person name...",
        //    Email = "person@example.com",
        //    Address = "sample address",
        //    CountryID = Guid.NewGuid(),
        //    Gender = GenderOptions.Male,
        //    DateOfBirth = DateTime.Parse("2000-01-01"),
        //    ReceiveNewsLetters = true
        //};

        //Arrange using AutoFixture
        //PersonAddRequest? personAddRequest = _fixture.Create<PersonAddRequest>();       //Create() may cause a failure due to random email address generation. Therefore we need Build()
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.Email, "someone@example.com").Create();


        //Act
        PersonResponse person_response_from_add = await _personsService.AddPerson(personAddRequest);

        List<PersonResponse> persons_list = await _personsService.GetAllPersons();

        //Assert
        Assert.True(person_response_from_add.PersonID != Guid.Empty);

        Assert.Contains(person_response_from_add, persons_list);
    }

    #endregion

    #region DeletePerson

    [Fact]
    public async Task DeletePerson_NullPersonID()
    {
        // Arrange
        Guid? personID = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _personsService.DeletePerson(personID));
    }

    [Fact]
    public async Task DeletePerson_InvalidPersonID()
    {
        // Arrange
        CountryAddRequest country_add_request = new CountryAddRequest { CountryName = "TestCountry" };
        var addedCountry = await _countriesService.AddCountry(country_add_request);

        var person_add_request = new PersonAddRequest
        {
            PersonName = "Test Person",
            Email = "test@example.com",
            CountryID = addedCountry.CountryID
        };
        var person_response_from_add = await _personsService.AddPerson(person_add_request);

        Guid? invalidPersonID = Guid.NewGuid();

        // Act
        bool result = await _personsService.DeletePerson(invalidPersonID);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeletePerson_ValidPersonID()
    {
        // Arrange
        CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
        CountryResponse countryResponse_from_add = await _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.PersonName, "Rahul").With(p => p.Email, "test@gmail.com").Create();


        PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);

        // Act
        bool result = await _personsService.DeletePerson(personResponse.PersonID);

        // Assert
        Assert.True(result);
        Assert.Null(await _personsService.GetPersonByPersonID(personResponse.PersonID));
    }

    #endregion
}