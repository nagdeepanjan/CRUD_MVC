using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private readonly ICountriesService _countriesService; //This field is needed by Person as a dependency
    private readonly List<Person> _persons;

    public PersonsService()
    {
        _persons = new List<Person>();
        _countriesService = new CountriesService(); //This is a dependency of Person
    }

    public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
    {
        if (personAddRequest == null) throw new ArgumentNullException(nameof(personAddRequest));


        //Model validations
        ValidationHelper.ModelValidation(personAddRequest);

        Person person = personAddRequest.ToPerson();
        person.PersonID = Guid.NewGuid();

        _persons.Add(person);

        //Now preparing to return PersonResponse object
        return ConvertPersonToPersonResponse(person);
    }

    public List<PersonResponse> GetAllPersons()
    {
        return _persons.Select(p => p.ToPersonResponse()).ToList();
    }

    public PersonResponse? GetPersonByPersonID(Guid? personID)
    {
        if (personID == null)
            return null;

        Person? person = _persons.FirstOrDefault(p => p.PersonID == personID);

        if (person == null)
            return null;

        return person.ToPersonResponse();
    }

    public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
    {
        throw new NotImplementedException();
    }

    //We spawned this method as it may be useful for many other situations, instead of just being used in AddPerson
    private PersonResponse ConvertPersonToPersonResponse(Person person)
    {
        PersonResponse personResponse = person.ToPersonResponse();
        personResponse.Country = _countriesService.GetCountryByCountryID(person.CountryID)?.CountryName;
        return personResponse;
    }
}