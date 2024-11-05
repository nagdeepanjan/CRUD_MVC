using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers;

public class PersonsController : Controller
{
    //private fields
    private readonly ICountriesService _countriesService;
    private readonly IPersonsService _personsService;

    public PersonsController(IPersonsService personsService, ICountriesService countriesService)
    {
        _personsService = personsService;
        _countriesService = countriesService;
    }

    [Route("persons/index")]
    [Route("/")]
    public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
    {
        #region Search

        List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);

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

        List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
        ViewBag.SortBy = sortBy;
        ViewBag.SortOrder = sortOrder.ToString(); //ToString() V.Imp for Sorting when heading is clicked

        #endregion

        return View(sortedPersons);
    }


    [Route("/persons/create")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        List<CountryResponse> countries = await _countriesService.GetAllCountries();
        ViewBag.Countries = countries;

        // populating the countries dropdown
        ViewBag.CountriesSelect = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryID.ToString() }).ToList();

        return View();
    }

    [Route("/persons/create")]
    [HttpPost]
    public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
    {
        if (ModelState.IsValid)
        {
            await _personsService.AddPerson(personAddRequest);
            return RedirectToAction("Index");
        }
        else
        {
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries;
            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return View(personAddRequest);
        }
    }

    [Route("/persons/edit/{personID}")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid personID)
    {
        PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

        if (personResponse == null) return RedirectToAction("Index");

        PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

        List<CountryResponse> countries = await _countriesService.GetAllCountries();
        ViewBag.Countries = countries;

        return View(personUpdateRequest);
    }

    [Route("/persons/edit/{personID}")] //This works if the view used asp-action. If you explicitly use the action name, personID is lost in the POST.
    [HttpPost]
    public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest)
    {
        PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

        if (personResponse == null) return RedirectToAction("Index");

        if (ModelState.IsValid)
        {
            PersonResponse updatedPerson = await _personsService.UpdatePerson(personUpdateRequest);
            return RedirectToAction("Index");
        }
        else
        {
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries;

            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return View(personResponse.ToPersonUpdateRequest());
        }
    }

    [Route("/persons/delete/{personID}")]
    [HttpGet]
    public async Task<IActionResult> Delete(Guid? personID)
    {
        PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

        if (personResponse == null) return RedirectToAction("Index");

        return View(personResponse);
    }

    [Route("/persons/delete/{personID}")]
    [HttpPost]
    public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
    {
        PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

        if (personResponse == null) return RedirectToAction("Index");

        await _personsService.DeletePerson(personUpdateRequest.PersonID);
        return RedirectToAction("Index");
    }
}