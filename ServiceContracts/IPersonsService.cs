using ServiceContracts.DTO;

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
}