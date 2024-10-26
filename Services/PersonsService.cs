using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

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

        if (string.IsNullOrEmpty(personAddRequest.PersonName))
            throw new ArgumentException("Person name is required", nameof(personAddRequest));

        Person person = personAddRequest.ToPerson();
        person.PersonID = Guid.NewGuid();

        _persons.Add(person);

        //Now preparing to return PersonResponse object
        return ConvertPersonToPersonResponse(person);
    }


    public List<PersonResponse> GetAllPersons()
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