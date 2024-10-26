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
}