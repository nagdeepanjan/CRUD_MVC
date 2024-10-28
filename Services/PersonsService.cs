﻿using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
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

        return matchingPerson.ToPersonResponse();
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