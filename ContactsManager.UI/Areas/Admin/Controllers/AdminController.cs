using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace ContactsManager.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]// only admin can access
    public class AdminController : Controller
    {
        private readonly IpersonServices _ipersonServices;

        public AdminController(IpersonServices ipersonServices, ILogger<AdminController> logger)
        {
            _ipersonServices = ipersonServices;
        }

        public async Task<IActionResult> AdminIndex()
        {
            var allPersons = await _ipersonServices.GetAllPersons();
            //now the get the ages of all persons
            //use dictionary to store the age of each person
            Dictionary<string, int> personAges = new Dictionary<string, int>
            {
                { "0-20", 0 },
                { "20-30", 0 },
                { "30-40", 0 },
                {"above 40", 0 }
            };

            //get the age of each person and store in the dictionary
            foreach (var person in allPersons)
            {
                var age = person.Age;
                if (age < 20)
                {
                    personAges["0-20"] += 1;
                }
                else if (age < 30)
                {
                    personAges["20-30"] += 1;
                }
                else if (age < 40)
                {
                    personAges["30-40"] += 1;
                }
                else
                {
                    personAges["above 40"] += 1;
                }
            }

            ViewBag.personAges = personAges;
            return View("AdminWelcomePage");
        }
     }
   }
