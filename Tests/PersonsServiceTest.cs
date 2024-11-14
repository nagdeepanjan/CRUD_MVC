using AutoFixture;
using Entities;
using Entities.DB;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace Tests;

public class PersonsServiceTest
{
    //private readonly ICountriesService _countriesService;
    private readonly IFixture _fixture;
    private readonly IPersonsRepository _personsRepository; //represents the mocked object that was created by Mock<IPersonsRepository>
    private readonly Mock<IPersonsRepository> _personsRepositoryMock; //used to mock the methods of IPersonsRepository
    private readonly IPersonsService _personsService;
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();

        _personsRepositoryMock = new Mock<IPersonsRepository>();
        _personsRepository = _personsRepositoryMock.Object;

        //Mocking Repository


        //Mocking DbContext
        DbContextMock<ApplicationDbContext> dbContextMock = new(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
        ApplicationDbContext dbContext = dbContextMock.Object;

        //Mocking DBSet        


        _personsService = new PersonsService(_personsRepository);

        _testOutputHelper = testOutputHelper;
    }

    #region GetSortedPersons

    [Fact]
    public async Task GetSortedPersons_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new()
        {
            _fixture.Build<Person>().With(p => p.Email, "m@gmail.com").With(p => p.Country, null as Country).Create(),
            _fixture.Build<Person>().With(p => p.Email, "n@gmail.com").With(p => p.Country, null as Country).Create(),
            _fixture.Build<Person>().With(p => p.Email, "o@gmail.com").With(p => p.Country, null as Country).Create()
        };

        List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();

        _personsRepositoryMock.Setup(x => x.GetAllPersons()).ReturnsAsync(persons);

        // Sorting by Name
        person_response_list_expected = person_response_list_expected.OrderBy(p => p.PersonName).ToList();

        List<PersonResponse> allPersons = await _personsService.GetAllPersons();

        // Print expected person response list
        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response in person_response_list_expected)
            _testOutputHelper.WriteLine(person_response.ToString());

        // Act
        List<PersonResponse> persons_list_from_sort =
            await _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.ASC);

        // Print actual person response list
        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_sort.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        // Assert
        persons_list_from_sort.Should().BeInAscendingOrder(p => p.PersonName);
    }

    #endregion

    #region UpdatePerson

    //When we supply null value as PersonUpdateRequest, it should throw ArgumentNullException
    [Fact]
    public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
    {
        //Arrange
        PersonUpdateRequest? personUpdateRequest = null;


        //Act
        Func<Task> act = async () => await _personsService.UpdatePerson(personUpdateRequest);

        //Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    //When invalid person id is supplied, it should throw ArgumentException
    [Fact]
    public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
    {
        //Arrange

        PersonUpdateRequest? personUpdateRequest = _fixture.Create<PersonUpdateRequest>(); //.Create();


        //Act
        Func<Task> act = async () => await _personsService.UpdatePerson(personUpdateRequest);
        //Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    //When personName is null, it should throw ArgumentException

    [Fact]
    public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
    {
        //Arrange


        Person person = _fixture.Build<Person>().With(p => p.PersonName, null as string).With(p => p.Email, "test@gmail.com").With(p => p.Country, null as Country).With(p => p.Gender, "Male")
            .Create();


        PersonResponse personResponse_expected = person.ToPersonResponse();

        PersonUpdateRequest personUpdateRequest = personResponse_expected.ToPersonUpdateRequest();

        _personsRepositoryMock.Setup(x => x.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
        _personsRepositoryMock.Setup(x => x.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(person);

        //personUpdateRequest.PersonName = null;


        //Act
        Func<Task> act = async () => await _personsService.UpdatePerson(personUpdateRequest);

        //Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    //Normal updating of person
    [Fact]
    public async Task UpdatePerson_ProperValues_ToBeSuccessful()
    {
        //Arrange

        Person person = _fixture.Build<Person>().With(p => p.Country, null as Country).With(p => p.Email, "test@gmail.com").With(p => p.Gender, "Male").Create();


        PersonResponse personResponse_expected = person.ToPersonResponse();

        PersonUpdateRequest personUpdateRequest = personResponse_expected.ToPersonUpdateRequest();

        _personsRepositoryMock.Setup(x => x.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
        _personsRepositoryMock.Setup(x => x.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(person);


        //Act
        PersonResponse personResponse_from_update = await _personsService.UpdatePerson(personUpdateRequest);


        //Assert
        //Assert.Equal(personResponse_from_update, personResponse_from_get);
        personResponse_from_update.Should().Be(personResponse_expected);
    }

    #endregion


    #region GetFilteredPersons

    //If searchBy is PersonName and searcgString is empty, it should return all persons
    [Fact]
    public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new()
        {
            _fixture.Build<Person>().With(p => p.Email, "m@gmail.com").With(p => p.Country, null as Country).Create(),
            _fixture.Build<Person>().With(p => p.Email, "n@gmail.com").With(p => p.Country, null as Country).Create(),
            _fixture.Build<Person>().With(p => p.Email, "o@gmail.com").With(p => p.Country, null as Country).Create()
        };

        List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();

        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response in person_response_list_expected)
            _testOutputHelper.WriteLine(person_response.ToString());

        _personsRepositoryMock.Setup(x => x.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

        // Act
        List<PersonResponse> persons_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");

        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_search.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        // Assert
        persons_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
    }

    //First we will add a few people and then search based on person name with some search string.
    [Fact]
    public async Task GetFilteredPersons_SearchByPersonName_ShouldBeSuccessful()
    {
        //Arrange
        List<Person> persons = new()
        {
            _fixture.Build<Person>().With(p => p.Email, "m@gmail.com").With(p => p.Country, null as Country).Create(),
            _fixture.Build<Person>().With(p => p.Email, "n@gmail.com").With(p => p.Country, null as Country).Create(),
            _fixture.Build<Person>().With(p => p.Email, "o@gmail.com").With(p => p.Country, null as Country).Create()
        };

        List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();

        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response in person_response_list_expected)
            _testOutputHelper.WriteLine(person_response.ToString()); //Using dependency injection


        _personsRepositoryMock.Setup(x => x.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);


        //Act
        List<PersonResponse> persons_list_from_search =
            await _personsService.GetFilteredPersons(nameof(Person.PersonName), "alpha");


        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_search.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        //Assert

        persons_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
    }

    #endregion

    #region GetAllPersons

    [Fact]
    public async Task GetAllPersons_ToBeEmptyList()
    {
        //Arrange
        _personsRepositoryMock.Setup(x => x.GetAllPersons()).ReturnsAsync(new List<Person>());

        //Act
        List<PersonResponse> persons = await _personsService.GetAllPersons();

        //Assert

        persons.Should().BeEmpty();
    }

    //Add a few persons and then GetAllPersons() should return the list of all persons
    [Fact]
    public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
    {
        //Arrange
        List<Person> persons = new()
        {
            _fixture.Build<Person>().With(p => p.Email, "email1@mail.com").With(p => p.Country, null as Country).Create(),
            _fixture.Build<Person>().With(p => p.Email, "email2@mail.com").With(p => p.Country, null as Country).Create(),
            _fixture.Build<Person>().With(p => p.Email, "email3@mail.com").With(p => p.Country, null as Country).Create()
        };


        //Act
        List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();


        //print person_response_list_from_add
        _testOutputHelper.WriteLine("Expected:");
        foreach (PersonResponse person_response_from_add in person_response_list_expected)
            _testOutputHelper.WriteLine(person_response_from_add.ToString()); //Using dependency injection


        _personsRepositoryMock.Setup(x => x.GetAllPersons()).ReturnsAsync(persons);

        //Act
        List<PersonResponse> persons_list_from_get = await _personsService.GetAllPersons();
        _testOutputHelper.WriteLine("Actual:");
        persons_list_from_get.ForEach(p => _testOutputHelper.WriteLine(p.ToString()));

        //Assert

        persons_list_from_get.Should().BeEquivalentTo(person_response_list_expected);
    }

    #endregion

    #region GetPersonByPersonID

    [Fact]
    public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
    {
        //Arrange
        Guid? personID = null;


        //Act
        PersonResponse? personResponse_from_get = await _personsService.GetPersonByPersonID(personID);

        //Assert

        personResponse_from_get.Should().BeNull();
    }

    //If we supply a valid person id, it should return the person object
    [Fact]
    public async Task GetPersonByPersonID_ValidPersonID_ToBeSuccessful()
    {
        //Arrange


        //Act


        //Autofixture creates a circular reference between Person and Country. So, we need to create a person object without country
        Person person = _fixture.Build<Person>().With(p => p.Email, "test@test.com").With(p => p.Country, null as Country).Create();

        PersonResponse person_response_expected = person.ToPersonResponse();


        _personsRepositoryMock.Setup(x => x.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(person);


        PersonResponse? person_response_from_get = await _personsService.GetPersonByPersonID(person.PersonID);

        //Act


        //Assert
        person_response_from_get.Should().Be(person_response_expected);
    }

    #endregion

    #region AddPerson

    [Fact]
    public async Task AddPerson_NullPerson_ToBeArgumentNullException()
    {
        //Arrange
        PersonAddRequest? personAddRequest = null;


        //Act
        Func<Task> act = async () => { await _personsService.AddPerson(personAddRequest); };

        //Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
    {
        //Arrange with AutoFixture

        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.PersonName, null as string).Create(); //null as string is important

        Person person = personAddRequest.ToPerson();

        //When PersonsRepository.AddPerson is called, it has to return the same person object
        _personsRepositoryMock.Setup(x => x.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

        //Act
        Func<Task> act = async () => { await _personsService.AddPerson(personAddRequest); };

        //Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    //When we supply proper person details, it should insert the person into the persons list; and it should return an object of PersonResponse, which includes with the newly generated person id
    [Fact]
    public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
    {
        //Arrange        
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.Email, "someone@example.com").Create();
        Person person = personAddRequest.ToPerson();
        PersonResponse person_response_expected = person.ToPersonResponse();


        _personsRepositoryMock.Setup(x => x.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

        //Act
        PersonResponse person_response_from_add = await _personsService.AddPerson(personAddRequest);
        person_response_expected.PersonID = person_response_from_add.PersonID;


        //Assert
        person_response_from_add.PersonID.Should().NotBe(Guid.Empty);
        person_response_from_add.Should().Be(person_response_expected);
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


        Person person_add_request = new Person
        {
            PersonName = "Test Person",
            Email = "test@example.com"
        };
        var person_response_from_add = person_add_request.ToPersonResponse();

        Guid? invalidPersonID = Guid.NewGuid();

        // Act
        bool result = await _personsService.DeletePerson(invalidPersonID);

        // Assert
        //Assert.False(result);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
    {
        // Arrange


        Person person = _fixture.Build<Person>()
            .With(p => p.Email, "test@gmail.com")
            .With(p => p.Country, null as Country)
            .With(p => p.Gender, "Male").Create();


        PersonResponse personResponse = person.ToPersonResponse();

        _personsRepositoryMock.Setup(x => x.DeletePersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(true);
        _personsRepositoryMock.Setup(x => x.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(person);

        // Act
        bool result = await _personsService.DeletePerson(personResponse.PersonID);

        // Assert

        result.Should().BeTrue();
    }
}

#endregion