using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        public HomeController(IPersonsService personsService, ICountriesService countriesService)
        {
            _personsService = personsService;
            _countriesService = countriesService;
        }

        public IActionResult Index()
        {
            var persons = _personsService.GetAllPersons();
            return View(persons);
        }

        public IActionResult Create()
        {
            ViewBag.Countries = _countriesService.GetAllCountries();
            return View();
        }

        [HttpPost]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            if (ModelState.IsValid)
            {
                _personsService.AddPerson(personAddRequest);
                return RedirectToAction("Index");
            }

            ViewBag.Countries = _countriesService.GetAllCountries();
            return View(personAddRequest);
        }

        public IActionResult Edit(Guid id)
        {
            var person = _personsService.GetPersonByPersonID(id);
            if (person == null)
            {
                return NotFound();
            }

            ViewBag.Countries = _countriesService.GetAllCountries();
            return View(person.ToPersonUpdateRequest());
        }

        [HttpPost]
        public IActionResult Edit(PersonUpdateRequest personUpdateRequest)
        {
            if (ModelState.IsValid)
            {
                _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }

            ViewBag.Countries = _countriesService.GetAllCountries();
            return View(personUpdateRequest);
        }

        public IActionResult Delete(Guid id)
        {
            var person = _personsService.GetPersonByPersonID(id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _personsService.DeletePerson(id);
            return RedirectToAction("Index");
        }
    }
}
