﻿using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts;

public interface IPersonsService
{
    PersonResponse AddPerson(PersonAddRequest? personAddRequest);

    List<PersonResponse> GetAllPersons();


    /// <summary>
    ///     Perturns the person object based on the person id
    /// </summary>
    /// <param name="personID"></param>
    /// <returns>returns the matching person object</returns>
    PersonResponse? GetPersonByPersonID(Guid? personID);


    /// <summary>
    ///     Returns all persom objects that matches with the given search field and search string
    /// </summary>
    /// <param name="searchBy">Search field to search</param>
    /// <param name="searchString">Value to search</param>
    /// <returns>All matching persons</returns>
    List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString);


    /// <summary>
    ///     Returns sorted list of persons
    /// </summary>
    /// <param name="allPersons">Represents list of persons to sort</param>
    /// <param name="sortBy">Name of property used to sort</param>
    /// <param name="sortOrder">ASC/DESC</param>
    /// <returns></returns>
    List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder);


    /// <summary>
    ///     Updates the specified person based on the given person ID
    /// </summary>
    /// <param name="personUpdateRequest">Person details to update, including person ID</param>
    /// <returns></returns>
    PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest);

    /// <summary>
    ///     Deletes a person based on the given person ID
    /// </summary>
    /// <param name="personID"></param>
    /// <returns>TRUE of successful</returns>
    bool DeletePerson(Guid? personID);
}