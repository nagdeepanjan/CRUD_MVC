using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters;

public class PersonsListActionFilter : IActionFilter
{
    private readonly ILogger<PersonsListActionFilter> _logger;

    public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("DEEPANJAN ==> PersonsListActionFilter.OnActionExecuted()");

        //Trying to access ViewData

        PersonsController personsController = (PersonsController)context.Controller;
        IDictionary<string, object?>? parameters = (IDictionary<string, object?>?)context.HttpContext.Items["arguments"];


        if (parameters != null && parameters.Count > 0)
        {
            if (parameters.ContainsKey("searchBy"))
                personsController.ViewData["searchBy"] = Convert.ToString(parameters["searchBy"]);

            if (parameters.ContainsKey("searchString"))
                personsController.ViewData["searchString"] = Convert.ToString(parameters["searchString"]);

            if (parameters.ContainsKey("sortBy"))
                personsController.ViewData["sortBy"] = Convert.ToString(parameters["sortBy"]);

            if (parameters.ContainsKey("sortOrder"))
                personsController.ViewData["sortOrder"] = Convert.ToString(parameters["sortOrder"]);
        }
        //The above could be used to negate the need for the controller to set ViewBags
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        //This one is to help OnActionExecuted
        context.HttpContext.Items["arguments"] = context.ActionArguments; //This can now be accessed from OnActionExecuted


        _logger.LogInformation("DEEPANJAN ==> PersonsListActionFilter.OnActionExecuting()");
        if (context.ActionArguments.ContainsKey("searchBy"))
        {
            string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);

            if (!string.IsNullOrEmpty(searchBy))
            {
                var searchByOptions = new List<string>
                {
                    nameof(PersonResponse.PersonName),
                    nameof(PersonResponse.Email),
                    nameof(PersonResponse.DateOfBirth),
                    nameof(PersonResponse.Gender),
                    nameof(PersonResponse.CountryID),
                    nameof(PersonResponse.Address)
                };
                //resetting the searchBy value
                if (!searchByOptions.Contains(searchBy))
                {
                    _logger.LogInformation("searchBy actual value {searchBy}", searchBy);
                    context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                    _logger.LogInformation("searchBy updated value {searchBy}", nameof(PersonResponse.PersonName));
                }
            }
        }
    }
}