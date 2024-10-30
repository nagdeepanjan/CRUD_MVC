using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers;

public class PersonsController : Controller
{
    //private fields
    private readonly IPersonsService _personsService;

    public PersonsController(IPersonsService personsService)
    {
        _personsService = personsService;
    }

    [Route("persons/index")]
    [Route("/")]
    public IActionResult Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
    {
        #region Search

        List<PersonResponse> persons = _personsService.GetFilteredPersons(searchBy, searchString);

        //Populating the search dropdown
        ViewBag.SearchFields = new Dictionary<string, string>
        {
            { nameof(PersonResponse.PersonName), "Person Name" },
            { nameof(PersonResponse.Email), "Email" },
            { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
            { nameof(PersonResponse.CountryID), "Country" },
            { nameof(PersonResponse.Gender), "Gender" },
            { nameof(PersonResponse.Address), "Address" }
        };

        ViewBag.SearchString = searchString; //Populating the search field
        ViewBag.SearchBy = searchBy; //Populating the search dropdown

        #endregion

        #region Sort

        List<PersonResponse> sortedPersons = _personsService.GetSortedPersons(persons, sortBy, sortOrder);
        ViewBag.SortBy = sortBy;
        ViewBag.SortOrder = sortOrder.ToString(); //ToString() V.Imp for Sorting when heading is clicked

        #endregion

        return View(sortedPersons);
    }
}