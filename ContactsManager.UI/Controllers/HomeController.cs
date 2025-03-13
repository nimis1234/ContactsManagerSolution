using DTO;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Helpers;
using System.ComponentModel.DataAnnotations;

namespace CURDExample.Controllers
{
    [Route("Home")]

    public class HomeController : Controller
    {
        private IcountryServices _countryServices;
        private IpersonServices _personServices;

        public HomeController(IcountryServices countryServices, IpersonServices personServices)
        {
            _countryServices = countryServices;
            _personServices = personServices;
        }

        
        [Route("Index")]
        [Route("/")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            
            return View();
        }

        [Route("AddPerson")]
        public IActionResult AddPerson(PersonAddRequest personAddRequest)
        {
            // before adding validate personAddrequest
            ModelValidation validatePerson = new ModelValidation();
            var validationResults = new List<ValidationResult>();
            bool isValid = ModelValidation.ValidateModel(personAddRequest, out validationResults);
            if (!isValid)
            {
                // Handle validation errors

                List<string> errors = new List<string>();
                foreach (var validationResult in validationResults)
                {
                    if (validationResult.ErrorMessage != null)
                    {
                        errors.Add(validationResult.ErrorMessage);
                    }
                }

                return BadRequest(new { Message = "Validation failed.", Errors = errors });
            }
            var result = _personServices.AddPerson(personAddRequest);
            return View();
        }

        [Route("Errors")]
        public IActionResult Errors(string message) // response come from exception get here
        {
            var errorMessage = HttpContext.Items["ErrorMessage"]?.ToString() ?? "An unexpected error occurred.";

            ViewBag.Message = errorMessage;
            return View();
        }

        [Route("Error")]
        public IActionResult Error(string message) // response come from exception get here
        {
            var errorMessage = HttpContext.Items["ErrorMessage"]?.ToString() ?? "An unexpected error occurred.";

            ViewBag.Message = errorMessage;
            return View("Error");
        }
    }
}
