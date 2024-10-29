using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private readonly ICountriesService _countriesService; //This field is needed by Person as a dependency
    private readonly List<Person> _persons;

    public PersonsService(bool initialize = false)
    {
        _persons = new List<Person>();
        _countriesService = new CountriesService(initialize); //This is a dependency of Person

        if (initialize)
        {
            var countries = _countriesService.GetAllCountries();
            _persons.AddRange(new List<Person>
            {
                new()
                {
                    PersonID = Guid.Parse("13BFBE9A-A250-4F2D-B3BB-41DA247C1C11"),
                    PersonName = "John Doe",
                    Email = "p1@gmail.com",
                    CountryID = countries[0].CountryID,
                    Address = "123, 1st Street, New York",
                    Gender = GenderOptions.Male.ToString(),
                    DateOfBirth = new DateTime(1990, 1, 1),
                    ReceiveNewsLetters = true
                },
                new()
                {
                    PersonID = Guid.Parse("8F751823-882A-4DFE-8EF8-3FEAF215CE55"),
                    PersonName = "Jill Tee",
                    Email = "p2@gmail.com",
                    CountryID = countries[1].CountryID,
                    Address = "234, 1st Street, Oregon",
                    Gender = GenderOptions.Female.ToString(),
                    DateOfBirth = new DateTime(1990, 1, 1),
                    ReceiveNewsLetters = true
                },
                new()
                {
                    PersonID = Guid.Parse("4C3D8781-5687-4815-B4FE-0677DA31DDDE"),
                    PersonName = "Peter Norton",
                    Email = "p3@gmail.com",
                    CountryID = countries[2].CountryID,
                    Address = "234, 13th Street, Florida",
                    Gender = GenderOptions.Male.ToString(),
                    DateOfBirth = new DateTime(1960, 1, 1),
                    ReceiveNewsLetters = false
                },
                new()
                {
                    PersonID = Guid.Parse("15AA0CC2-40B8-49C5-8DA1-BC64C49A8B6A"),
                    PersonName = "Lee Morgan",
                    Email = "p4@gmail.com",
                    CountryID = countries[3].CountryID,
                    Address = "234, 1st Street, Connecticut",
                    Gender = GenderOptions.Male.ToString(),
                    DateOfBirth = new DateTime(1980, 1, 1),
                    ReceiveNewsLetters = true
                },
                new()
                {
                    PersonID = Guid.Parse("27381194-EAF6-4E28-A033-7C857AEFA373"),
                    PersonName = "Lady Ada",
                    Email = "p5@hotmail.com",
                    CountryID = countries[4].CountryID,
                    Address = "Alpha, 1st Street, New Jersey",
                    Gender = GenderOptions.Female.ToString(),
                    DateOfBirth = new DateTime(1980, 4, 4),
                    ReceiveNewsLetters = false
                },
                new()
                {
                    PersonID = Guid.Parse("BA353F8D-D0A3-4323-8ED0-8FDB32A2F219"),
                    PersonName = "Ravi Kumar",
                    Email = "p6@gmail.com",
                    CountryID = countries[0].CountryID,
                    Address = "321, 1st Street, New York",
                    Gender = GenderOptions.Male.ToString(),
                    DateOfBirth = new DateTime(1960, 1, 1),
                    ReceiveNewsLetters = true
                },
                new()
                {
                    PersonID = Guid.Parse("B67C966B-965B-44CD-8BD1-F7E6FBCADB26"),
                    PersonName = "Sita Kaur",
                    Email = "p7@gmail.com",
                    CountryID = countries[1].CountryID,
                    Address = "456, 1st Street, Oregon",
                    Gender = GenderOptions.Female.ToString(),
                    DateOfBirth = new DateTime(1950, 1, 1),
                    ReceiveNewsLetters = true
                },
                new()
                {
                    PersonID = Guid.Parse("8D1E7BCE-B8E4-42C0-B178-DF573190E1B0"),
                    PersonName = "Ujwal Banerjee",
                    Email = "p8@gmail.com",
                    CountryID = countries[2].CountryID,
                    Address = "654, 13th Street, Florida",
                    Gender = GenderOptions.Male.ToString(),
                    DateOfBirth = new DateTime(1990, 1, 1),
                    ReceiveNewsLetters = false
                },
                new()
                {
                    PersonID = Guid.Parse("67CC8CAC-23AB-4803-98C5-C2EEDD3F611B"),
                    PersonName = "Santosh Singh",
                    Email = "p9@gmail.com",
                    CountryID = countries[3].CountryID,
                    Address = "2379, 1st Street, Connecticut",
                    Gender = GenderOptions.Male.ToString(),
                    DateOfBirth = new DateTime(1975, 1, 1),
                    ReceiveNewsLetters = true
                },
                new()
                {
                    PersonID = Guid.Parse("FCF2AF1D-1D53-475D-AB89-30E745626BCC"),
                    PersonName = "Prashant Kuleshwar",
                    Email = "p10@hotmail.com",
                    CountryID = countries[4].CountryID,
                    Address = "Beta, 1st Street, New Jersey",
                    Gender = GenderOptions.Female.ToString(),
                    DateOfBirth = new DateTime(1984, 4, 4),
                    ReceiveNewsLetters = false
                }
            });
        }
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
        return _persons.Select(p => ConvertPersonToPersonResponse(p)).ToList();
    }

    public PersonResponse? GetPersonByPersonID(Guid? personID)
    {
        if (personID == null)
            return null;

        Person? person = _persons.FirstOrDefault(p => p.PersonID == personID);

        if (person == null)
            return null;

        return ConvertPersonToPersonResponse(person);
    }

    public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
    {
        List<PersonResponse> allPersons = GetAllPersons();
        List<PersonResponse> matchingPersons = allPersons;

        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
            return matchingPersons;

        switch (searchBy)
        {
            case nameof(Person.PersonName):
                matchingPersons = allPersons.Where(p =>
                    p.PersonName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(Person.Email):
                matchingPersons = allPersons
                    .Where(p => p.Email?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(Person.DateOfBirth):
                matchingPersons = allPersons.Where(p =>
                    p.DateOfBirth?.ToString("dd MMMM yyyy")
                        .Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(Person.Gender):
                matchingPersons = allPersons
                    .Where(p => p.Gender?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(Person.CountryID):
                matchingPersons = allPersons.Where(p =>
                    p.Country?.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(Person.Address):
                matchingPersons = allPersons
                    .Where(p => p.Address?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            default:
                return allPersons;
        }

        return matchingPersons;
    }

    public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
    {
        if (string.IsNullOrEmpty(sortBy))
            return allPersons;

        List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
        {
            (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.Email), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.Email), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.DateOfBirth).ToList(),
            (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.DateOfBirth).ToList(),
            (nameof(PersonResponse.Age), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Age).ToList(),
            (nameof(PersonResponse.Age), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Age).ToList(),
            (nameof(PersonResponse.Gender), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Gender).ToList(),
            (nameof(PersonResponse.Gender), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Gender).ToList(),
            (nameof(PersonResponse.Country), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.Country), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.Address), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.Address), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.ReceiveNewsLetters).ToList(),
            (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.ReceiveNewsLetters).ToList(),
            _ => allPersons
        };
        return sortedPersons;
    }

    public PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
        if (personUpdateRequest == null) throw new ArgumentNullException(nameof(personUpdateRequest));

        //PersonUpdateRequest is already annotated with validation attributes
        //We just need to call the ModelValidation method to validate the object
        ValidationHelper.ModelValidation(personUpdateRequest); //Throws ARGUMENT EXCEPTION if validation fails

        //get matching person object to update
        Person? matchingPerson = _persons.FirstOrDefault(p => p.PersonID == personUpdateRequest.PersonID);

        if (matchingPerson == null)
            throw new ArgumentException("Person not found", nameof(personUpdateRequest));

        //Update the matching person object
        matchingPerson.PersonName = personUpdateRequest.PersonName;
        matchingPerson.CountryID = personUpdateRequest.CountryID;
        matchingPerson.Email = personUpdateRequest.Email;
        matchingPerson.PersonName = personUpdateRequest.PersonName;
        matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
        matchingPerson.Gender = personUpdateRequest?.Gender.ToString();
        matchingPerson.Address = personUpdateRequest?.Address;
        matchingPerson.ReceiveNewsLetters = personUpdateRequest?.ReceiveNewsLetters ?? false;

        return ConvertPersonToPersonResponse(matchingPerson);
    }

    public bool DeletePerson(Guid? personID)
    {
        if (personID == null) throw new ArgumentNullException(nameof(personID));

        Person? person = _persons.FirstOrDefault(p => p.PersonID == personID);

        if (person == null) return false;

        _persons.Remove(person);
        return true;
    }

    //We spawned this method as it may be useful for many other situations, instead of just being used in AddPerson
    private PersonResponse ConvertPersonToPersonResponse(Person person)
    {
        PersonResponse personResponse = person.ToPersonResponse();
        personResponse.Country = _countriesService.GetCountryByCountryID(person.CountryID)?.CountryName;
        return personResponse;
    }
}