using Entities;
using Entities.DB;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private readonly ICountriesService _countriesService; //This field is needed by Person as a dependency

    //private readonly List<Person> _persons;
    private readonly PersonsDbContext _db;

    public PersonsService(PersonsDbContext personsDbContext, ICountriesService countriesService) //bool initialize = false
    {
        _db = personsDbContext;
        _countriesService = countriesService;
    }

    public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
    {
        if (personAddRequest == null) throw new ArgumentNullException(nameof(personAddRequest));

        //Model validations
        ValidationHelper.ModelValidation(personAddRequest);

        Person person = personAddRequest.ToPerson();
        person.PersonID = Guid.NewGuid();

        await _db.Persons.AddAsync(person);
        await _db.SaveChangesAsync();

        //Using SP
        //_db.sp_InsertPerson(person);

        //Now preparing to return PersonResponse object
        return await ConvertPersonToPersonResponse(person);
    }

    public async Task<List<PersonResponse>> GetAllPersons()
    {
        var persons = await _db.Persons.ToListAsync();
        var personResponses = new List<PersonResponse>();

        foreach (var person in persons) personResponses.Add(await ConvertPersonToPersonResponse(person));

        return personResponses;
    }

    public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
    {
        if (personID == null)
            return null;

        Person? person = await _db.Persons.Include("Country").FirstOrDefaultAsync(p => p.PersonID == personID);

        if (person == null)
            return null;

        return await ConvertPersonToPersonResponse(person);
    }

    public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
    {
        List<PersonResponse> allPersons = await GetAllPersons();
        List<PersonResponse> matchingPersons = allPersons;

        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
            return matchingPersons;

        switch (searchBy)
        {
            case nameof(PersonResponse.PersonName):
                matchingPersons = allPersons.Where(p =>
                    p.PersonName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(PersonResponse.Email):
                matchingPersons = allPersons
                    .Where(p => p.Email?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(PersonResponse.DateOfBirth):
                matchingPersons = allPersons.Where(p =>
                    p.DateOfBirth?.ToString("dd MMMM yyyy")
                        .Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(PersonResponse.Gender):
                matchingPersons = allPersons
                    .Where(p => p.Gender?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(PersonResponse.CountryID):
                matchingPersons = allPersons.Where(p =>
                    p.Country?.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            case nameof(PersonResponse.Address):
                matchingPersons = allPersons
                    .Where(p => p.Address?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList();
                break;
            default:
                return allPersons;
        }

        return matchingPersons;
    }

    public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
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
        return await Task.FromResult(sortedPersons);
    }

    public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
        if (personUpdateRequest == null) throw new ArgumentNullException(nameof(personUpdateRequest));

        //PersonUpdateRequest is already annotated with validation attributes
        //We just need to call the ModelValidation method to validate the object
        ValidationHelper.ModelValidation(personUpdateRequest); //Throws ARGUMENT EXCEPTION if validation fails

        //get matching person object to update
        Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == personUpdateRequest.PersonID);

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

        await _db.SaveChangesAsync();

        return await ConvertPersonToPersonResponse(matchingPerson);
    }

    public async Task<bool> DeletePerson(Guid? personID)
    {
        if (personID == null) throw new ArgumentNullException(nameof(personID));

        Person? person = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == personID);

        if (person == null) return false;

        _db.Persons.Remove(person);
        await _db.SaveChangesAsync();
        return true;
    }

    //We spawned this method as it may be useful for many other situations, instead of just being used in AddPerson
    //Helps in getting CountryName from CountryID
    private async Task<PersonResponse> ConvertPersonToPersonResponse(Person person)
    {
        PersonResponse personResponse = person.ToPersonResponse();
        var country = await _countriesService.GetCountryByCountryID(person.CountryID);
        personResponse.Country = country?.CountryName;
        return personResponse;
    }
}