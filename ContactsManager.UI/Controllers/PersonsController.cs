using CURDExample.Filters;
using DTO;
using DTO.Enums;
using Exceptions;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using Rotativa.AspNetCore;
using Services;
using Services.Helpers;

namespace CURDExample.Controllers
{
    [Route("Persons")]
   
    public class PersonsController : Controller
    {
        private IcountryServices _icountryServices;
        private IpersonServices _ipersonServices;
        private readonly ILogger<PersonsController> _logger;
        private bool isDeleted;

        public PersonsController(IcountryServices icountryServices, IpersonServices ipersonServices, ILogger<PersonsController> logger)
        {
            _icountryServices = icountryServices;
            _ipersonServices = ipersonServices;
            _logger = logger;
        }




        [Route("Index")]
        public async Task<IActionResult> Index(string? searchBy, string? searchString, string? sortOrder, string? sortBy)
        {
            try
            {
                
                _logger.LogInformation($"Index action called with searchBy {searchBy} , searchString {searchString}, sortBy {sortBy}, sortOrder {sortOrder}");
                IEnumerable<PersonResponse?> result;

                ViewBag.SearchFields = new Dictionary<string, string>()
              {
                { nameof(PersonResponse.PersonName), "Person Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                { nameof(PersonResponse.Gender), "Gender" },
                { nameof(PersonResponse.CountryID), "Country" },
                { nameof(PersonResponse.Address), "Address" }
              };

                //view bag column header and display name

                ViewBag.ColumnHeaders = new Dictionary<string, string>
                {
                    { nameof(PersonResponse.PersonName), "Name" },
                    { nameof(PersonResponse.Email), "Email" },
                    { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                    { nameof(PersonResponse.Gender), "Gender" },
                    { nameof(PersonResponse.Country), "Country" },
                    { nameof(PersonResponse.Address), "Address" },
                    { nameof(PersonResponse.ReceiveNewsLetters), "Receive Newsletters" },
                    { nameof(PersonResponse.Age), "Age" },
                    { "Other", "Edit/Delete" }
                };



                if (!string.IsNullOrEmpty(searchBy) || !string.IsNullOrEmpty(searchString))
                {
                    ViewBag.CurrentSearchBy = searchBy;
                    ViewBag.CurrentSearchString = searchString;
                    _logger.LogInformation($"GetFilteredPersons action called with searchBy {searchBy} , searchString {searchString}");
                    result = await _ipersonServices.GetFilteredPersons(searchBy, searchString);
                    return View(result);
                }

                else
                {
                    result = await _ipersonServices.GetAllPersons();
                    if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
                    {
                        ViewBag.CurrentSortBy = sortBy;
                        ViewBag.CurrentSortOrder = sortOrder;
                        _logger.LogInformation($"GetSortedData action called with sortBy {sortBy} , sortOrder {sortOrder}");
                        result = _ipersonServices.GetSortedData(result.Where(x => x != null).Cast<PersonResponse>().ToList(), sortBy, (SortOrderOption)Enum.Parse(typeof(SortOrderOption), sortOrder, true));
                    }
                    return View(result);

                }
            }
            catch (CustomExceptions ex) // example how to catch custom exception
            {
                _logger.LogError(ex, "An error occurred in Index action");
                return BadRequest(new { message = ex.Message, innnerException = ex.InnerException });
            }

        }



        [Route("CreatePerson")]
        [HttpGet]
        public async Task<IActionResult> CreatePerson()
        {
            try
            {
                _logger.LogInformation($"CreatePerson action called");

                // put into selectedlistitem for dropdown
                ViewBag.Countries = (await _icountryServices.GetAllCountries())
                    .Select
                    (c => new SelectListItem
                    {
                        Value = c.CountryId.ToString(),
                        Text = c.CountryName
                    }).ToList();

                _logger.LogInformation($"CreatePerson action called");
                return View();
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "An error occurred in CreatePerson action");
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
        }

        [Route("CreatePerson")]
        [HttpPost]
        [ModalValidationFilterFactory(1)] // filter ,currenlty used ifilter factory , you can use service filter instead both are okay 
        public async Task<IActionResult> CreatePerson(PersonAddRequest personAddRequest)
        {

            var isValidModel = ModelValidation.ValidateModel(personAddRequest, out var validationResults);

            // put into selectedlistitem for dropdown
            ViewBag.Countries = (await _icountryServices.GetAllCountries())
                .Select
                (c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryName
                }).ToList();

            if (!isValidModel)
            {
                ViewBag.Errors = validationResults.Where(x => x.ErrorMessage != null).Select(x => x.ErrorMessage).ToList();
                return View();
            }
            else
                await _ipersonServices.AddPerson(personAddRequest);

            return RedirectToAction("Index", "Persons");

        }




        [Route("EditPerson/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditPerson(Guid id)
        {
            var person = await _ipersonServices.GetPersonById(id);

            // put into selectedlistitem for dropdown
            ViewBag.Countries = (await _icountryServices.GetAllCountries())
                .Select
                (c => new SelectListItem
                {
                    Value = c.CountryId.ToString(),
                    Text = c.CountryName
                }).ToList();

            return View(person);
        }




        [Route("EditPerson/{id}")]
        [HttpPost]
        public async Task<IActionResult> EditPerson(PersonUpdateRequest personUpdateRequest)
        {
            var isValidModel = ModelValidation.ValidateModel(personUpdateRequest, out var validationResults);
            if (!isValidModel)
            {
                ViewBag.Errors = validationResults.Where(x => x.ErrorMessage != null).Select(x => x.ErrorMessage).ToList();
                return View();
            }
            else
            {
                await _ipersonServices.UpdatePerson(personUpdateRequest);
            }
            return RedirectToAction("Index");
        }




        //delete person 
        [Route("DeletePerson/{id}")]
        [HttpGet]
        public async Task<IActionResult> DeletePerson(Guid id)
        {
            try
            {

                if (id == null)
                {
                    throw new ArgumentNullException(nameof(id));
                }
                else

                {
                    var person = await _ipersonServices.GetPersonById(id);
                    if (person == null)
                    {
                        throw new ArgumentException($"Person with id {id} does not exist");
                    }
                    isDeleted = await _ipersonServices.DeletePerson(id);


                }
                if (!isDeleted)
                {
                    return NotFound($"Person with id {id} does not exist");
                }
            }

            catch (CustomExceptions ex)
            {
                Console.WriteLine("Error in DeletePerson action: " + ex.Message);
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
            Console.WriteLine("DeletePerson action completed");
            return Json(new { message = "Test successful" });
            //return Json(new { redirectUrl = Url.Action("CreatePerson", "Persons") });


        }

        [Route("ExportToPdf")]
        public async Task<IActionResult> ExportToPdf()
        {
            var persons = await _ipersonServices.GetAllPersons();
            //viewsaspdf is used to convert view to pdf and we can settings to pdf like filename, page margin, page orientation
            return new ViewAsPdf("PersonToPDF", persons, ViewData)
            {
                FileName = "Persons.pdf",
                PageMargins = new Rotativa.AspNetCore.Options.Margins() { Top = 20, Right = 20, Bottom = 20, Left = 20 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape

            };

        }


        [Route("ExportToExcel")]

        //install EPPlus to export to excel in service project
        public async Task<IActionResult> ExportToExcel()
        {
            var excelMemoryStream = await _ipersonServices.ExportToExcel();
            return File(excelMemoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");

        }


        [Route("ExportToCSV")]

        //install EPPlus to export to excel in service project
        public async Task<IActionResult> ExportToCSV()
        {
            var CSVMemoryStream = await _ipersonServices.ExportToCSV();
            return File(CSVMemoryStream, "text/csv", "persons.csv");

        }


        [Route("UploadCountries")]
        [HttpGet]
        public async Task<IActionResult> UploadCountries()
        {
            var countries = await _icountryServices.GetAllCountries();
            return View("UploadCountryExcel", countries);
        }



        [Route("UploadCountries")]
        [HttpPost]
        public async Task<IActionResult> UploadCountries(IFormFile file)
        {
            var countries = await _icountryServices.GetAllCountries();
            var result = await _icountryServices.ExtractCountryExcelFile(file);

            if (result != null)
            {

                // check containes error
                foreach (var item in result)
                {
                    var error = item;
                    ViewBag.Error = error;
                }

                return View("UploadCountryExcel", countries);

            }
            return View("UploadCountryExcel", countries);
        }

        [Route("DownloadSampleExcel")]

        [HttpGet]
        public IActionResult DownloadSampleExcel()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Countries");
                worksheet.Cells[1, 1].Value = "Mexico";
                worksheet.Cells[2, 1].Value = "United States";
                worksheet.Cells[3, 1].Value = "Canada";
                worksheet.Cells[4, 1].Value = "United Kingdom";

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = "Sample_Countries.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

        [Route("Test")]
        [HttpGet]
        public IActionResult Test()
        {
            Console.WriteLine("Test endpoint called"); // Debugging line
            return Json(new { message = "Test successful" });
        }

        [Route("Error")]
        public IActionResult Error(string message) // response come from exception get here
        {
            var errorMessage = HttpContext.Items["ErrorMessage"]?.ToString() ?? "An unexpected error occurred.";

            ViewBag.Message = errorMessage;
            return View();
        }

    }
}