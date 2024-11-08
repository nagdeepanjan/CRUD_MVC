using Entities;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    //private readonly List<Person> _persons;
    private readonly IPersonsRepository _personsRepository;

    public PersonsService(IPersonsRepository personsRepository) //bool initialize = false
    {
        _personsRepository = personsRepository;
    }

    public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
    {
        if (personAddRequest == null) throw new ArgumentNullException(nameof(personAddRequest));

        //Model validations
        ValidationHelper.ModelValidation(personAddRequest);

        Person person = personAddRequest.ToPerson();
        person.PersonID = Guid.NewGuid();

        await _personsRepository.AddPerson(person);

        return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetAllPersons()
    {
        var persons = await _personsRepository.GetAllPersons();

        var listOfPersonResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();
        return listOfPersonResponse;
    }

    public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
    {
        if (personID == null)
            return null;

        Person? person = await _personsRepository.GetPersonByPersonID(personID.Value);

        if (person == null)
            return null;
        return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
    {
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
            return await GetAllPersons();

        var persons = await _personsRepository.GetFilteredPersons(p =>
            (searchBy == nameof(PersonResponse.PersonName) && p.PersonName != null && p.PersonName.Contains(searchString)) ||
            (searchBy == nameof(PersonResponse.Email) && p.Email != null && p.Email.Contains(searchString)) ||
            (searchBy == nameof(PersonResponse.DateOfBirth) && p.DateOfBirth != null && p.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchString)) ||
            (searchBy == nameof(PersonResponse.Gender) && p.Gender != null && p.Gender.Contains(searchString)) ||
            (searchBy == nameof(PersonResponse.CountryID) && p.Country != null && p.Country.CountryName != null && p.Country.CountryName.Contains(searchString)) ||
            (searchBy == nameof(PersonResponse.Address) && p.Address != null && p.Address.Contains(searchString))
        );
        return persons.Select(p => p.ToPersonResponse()).ToList();
    }

    public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
    {
        if (string.IsNullOrEmpty(sortBy))
            return allPersons;

        List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
        {
            (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.PersonName).ToList(),
            (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.PersonName).ToList(),
            (nameof(PersonResponse.Email), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Email).ToList(),
            (nameof(PersonResponse.Email), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Email).ToList(),
            (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.DateOfBirth).ToList(),
            (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.DateOfBirth).ToList(),
            (nameof(PersonResponse.Age), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Age).ToList(),
            (nameof(PersonResponse.Age), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Age).ToList(),
            (nameof(PersonResponse.Gender), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Gender).ToList(),
            (nameof(PersonResponse.Gender), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Gender).ToList(),
            (nameof(PersonResponse.Country), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Country).ToList(),
            (nameof(PersonResponse.Country), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Country).ToList(),
            (nameof(PersonResponse.Address), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Address).ToList(),
            (nameof(PersonResponse.Address), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Address).ToList(),
            (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.ReceiveNewsLetters).ToList(),
            (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.ReceiveNewsLetters).ToList(),
            _ => allPersons
        };
        return await Task.FromResult(sortedPersons);
    }

    public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
        if (personUpdateRequest == null) throw new ArgumentNullException(nameof(personUpdateRequest));

        //PersonUpdateRequest is already annotated with validation attributes
        //We just need to call the ModelValidation method to validate the object
        ValidationHelper.ModelValidation(personUpdateRequest); //Throws ARGUMENT EXCEPTION if validation fails

        //get matching person object to update
        Person? matchingPerson = await _personsRepository.GetPersonByPersonID(personUpdateRequest.PersonID);

        if (matchingPerson == null)
            throw new ArgumentException("Person not found", nameof(personUpdateRequest));

        //Update the matching person object
        matchingPerson.PersonName = personUpdateRequest.PersonName;
        matchingPerson.CountryID = personUpdateRequest.CountryID;
        matchingPerson.Email = personUpdateRequest.Email;
        matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
        matchingPerson.Gender = personUpdateRequest?.Gender.ToString();
        matchingPerson.Address = personUpdateRequest?.Address;
        matchingPerson.ReceiveNewsLetters = personUpdateRequest?.ReceiveNewsLetters ?? false;

        await _personsRepository.UpdatePerson(matchingPerson);
        return matchingPerson.ToPersonResponse();
    }

    public async Task<bool> DeletePerson(Guid? personID)
    {
        if (personID == null) throw new ArgumentNullException(nameof(personID));

        var person = await _personsRepository.GetPersonByPersonID(personID.Value);

        if (person == null) return false;

        await _personsRepository.DeletePersonByPersonID(personID.Value);
        return true;
    }
}