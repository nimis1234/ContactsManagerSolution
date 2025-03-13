using CsvHelper;
using DTO;
using DTO.Enums;
using Entities;
using Exceptions;
using Interfaces;
using IRepository;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PersonServices : IpersonServices
    {
        private List<Person?> _personList;
        private readonly IcountryServices _countryServices;
        private readonly IPersonRepo _personRepo;
        private readonly ILogger<PersonServices> _logger;

        public PersonServices(IPersonRepo personRepo, IcountryServices countryServices, ILogger<PersonServices> logger)
        {
            _personList = new List<Person>();
            _countryServices = countryServices;
            _personRepo = personRepo;
            _logger = logger;
        }

        public async Task<PersonResponse?> AddPerson(PersonAddRequest? person)
        {
            try
            {
                if (person == null)
                {
                   throw new ArgumentNullException(nameof(person));
                }

                var convertToPerson = person.ToPerson();
                convertToPerson.PersonID = Guid.NewGuid();
                await _personRepo.AddPerson(convertToPerson);

                var personResponse = convertToPerson.ToPersonResponse();
                personResponse.Country = (await _countryServices.GetCountryById(personResponse.CountryID))?.CountryName;
                return personResponse;
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error adding person");
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest personUpdateRequest)
        {
            try
            {
                if (personUpdateRequest == null)
                {
                    _logger.LogWarning("personUpdateRequest is null");
                    throw new ArgumentNullException(nameof(personUpdateRequest));
                }

                Person person = personUpdateRequest.ToPerson();
                var personToUpdate = await _personRepo.UpdatePerson(person);
                var isValid = ModelValidation.ValidateModel(personUpdateRequest, out var validationResults);
                if (!isValid)
                {
                    _logger.LogWarning("Model validation failed");
                    throw new ArgumentException($"Model Validation error : {string.Join(", ", validationResults.Select(x => x.ErrorMessage))}");
                }
                _logger.LogInformation("Person updated successfully");
                return personToUpdate.ToPersonResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating person");
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
        }


        public List<PersonResponse> GetSortedData(List<PersonResponse> allPersons, string sortBy, SortOrderOption sortOrder)
        {
            try
            {
                _logger.LogInformation("Sorting data based on {sortBy} and {sortOrder}", sortBy, sortOrder);
                if (string.IsNullOrEmpty(sortBy))
                    return allPersons;

                // refer switch expression another way to write switch case
                List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
                {

                    (nameof(PersonResponse.PersonName), SortOrderOption.ASC) => allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
                    (nameof(PersonResponse.PersonName), SortOrderOption.DESC) => allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
                    (nameof(PersonResponse.Email), SortOrderOption.ASC) => allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),
                    (nameof(PersonResponse.Email), SortOrderOption.DESC) => allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),
                    (nameof(PersonResponse.Address), SortOrderOption.ASC) => allPersons.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),
                    (nameof(PersonResponse.Address), SortOrderOption.DESC) => allPersons.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),
                    (nameof(PersonResponse.DateOfBirth), SortOrderOption.ASC) => allPersons.OrderBy(temp => temp.DateOfBirth).ToList(),
                    (nameof(PersonResponse.DateOfBirth), SortOrderOption.DESC) => allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList(),
                    (nameof(PersonResponse.Gender), SortOrderOption.ASC) => allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),
                    _ => allPersons  // else return allPersons
                };

                return sortedPersons;
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error sorting data");
                //throw new CustomExceptions(ex.Message, ex.InnerException);
                return new List<PersonResponse>();
            }

        }

        public async Task<List<PersonResponse?>> GetAllPersons()
        {
            try
            {
                _personList = await _personRepo.GetAllPersons();
                var _personlistResponse = _personList
                    .Where(person => person != null)
                    .Select(person => person!.ToPersonResponse())
                    .ToList();

                foreach (var person in _personlistResponse)
                {
                    person.Country = (await _countryServices.GetCountryById(person.CountryID))?.CountryName;
                }
                return _personlistResponse;
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error retrieving all persons");
                return new List<PersonResponse?>();
            }
        }


        public async Task<List<PersonResponse>> GetFilteredPersons(string? searchBy, string? SearchString)
        {
            var getAllPersons = await GetAllPersons();
            var filteredPersons = new List<PersonResponse>();
            if (getAllPersons == null)
            {
                return filteredPersons;
            }
            filteredPersons = getAllPersons;
            if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(SearchString))
            {
                return filteredPersons;
            }

            switch (searchBy)
            {
                case nameof(PersonResponse.PersonName):
                    filteredPersons = filteredPersons.Where(temp => !(string.IsNullOrEmpty(temp.PersonName)) ? temp.PersonName.Contains(SearchString, StringComparison.OrdinalIgnoreCase) : true).ToList(); //string comparison to ignore case


                    break;
                case nameof(PersonResponse.Email):
                    filteredPersons = filteredPersons.Where(temp => !(string.IsNullOrEmpty(temp.Email)) ? temp.Email.Contains(SearchString, StringComparison.OrdinalIgnoreCase) : true).ToList(); //string comparison to ignore case
                    break;
                case nameof(PersonResponse.Address):
                    filteredPersons = filteredPersons.Where(temp => !(string.IsNullOrEmpty(temp.Address)) ? temp.Address.Contains(SearchString, StringComparison.OrdinalIgnoreCase) : true).ToList(); //string comparison to ignore case
                    break;
                case nameof(PersonResponse.Country):
                    filteredPersons = filteredPersons.Where(temp => !(string.IsNullOrEmpty(temp.Country)) ? temp.Country.Contains(SearchString, StringComparison.OrdinalIgnoreCase) : true).ToList(); //string comparison to ignore case
                    break;

                case nameof(PersonResponse.DateOfBirth):
                    filteredPersons = filteredPersons.Where(temp => temp.DateOfBirth.HasValue ? temp.DateOfBirth.Value.ToString("yyyy-MM-dd").Contains(SearchString, StringComparison.OrdinalIgnoreCase) : true).ToList(); //string comparison to ignore case
                    break;
            }

            return filteredPersons;
        }
        public async Task<PersonResponse?> GetPersonById(Guid? id)
        {
            try
            {
                if (id == null) throw new ArgumentNullException(nameof(id));
                var person = await _personRepo.GetPersonById(id);
                return person?.ToPersonResponse();
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error retrieving person by ID: {Id}", id);
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
        }

        public async Task<bool> DeletePerson(Guid? id)
        {
            try
            {
                if (id == null) throw new ArgumentNullException(nameof(id));
                return await _personRepo.DeletePerson(id);
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error deleting person with ID: {Id}", id);
                return false;
            }
        }

        public async Task<MemoryStream> ExportToCSV()
        {
            try
            {
                var memoryStream = new MemoryStream();
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    var personResponses = await GetAllPersons();
                    csv.WriteHeader<PersonResponse>();
                    await csv.NextRecordAsync();
                    await csv.WriteRecordsAsync(personResponses);
                    await writer.FlushAsync();
                }
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error exporting persons to CSV");
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
        }

        public async Task<MemoryStream?> ExportToExcel()
        {
            try
            {
                var memoryStream = new MemoryStream();
                var personList = await GetAllPersons();
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("PersonList");
                    worksheet.Cells[1, 1].Value = "PersonID";
                    worksheet.Cells[1, 2].Value = "PersonName";
                    worksheet.Cells[1, 3].Value = "Email";
                    worksheet.Cells[1, 4].Value = "DateOfBirth";
                    worksheet.Cells[1, 5].Value = "Gender";
                    worksheet.Cells[1, 6].Value = "Country";
                    worksheet.Cells[1, 7].Value = "Address";
                    worksheet.Cells[1, 8].Value = "ReceiveNewsLetters";

                    for (int i = 0; i < personList.Count; i++)
                    {
                        var person = personList[i];
                        worksheet.Cells[i + 2, 1].Value = person.PersonID;
                        worksheet.Cells[i + 2, 2].Value = person.PersonName;
                        worksheet.Cells[i + 2, 3].Value = person.Email;
                        worksheet.Cells[i + 2, 4].Value = person.DateOfBirth;
                        worksheet.Cells[i + 2, 5].Value = person.Gender;
                        worksheet.Cells[i + 2, 6].Value = person.Country;
                        worksheet.Cells[i + 2, 7].Value = person.Address;
                        worksheet.Cells[i + 2, 8].Value = person.ReceiveNewsLetters;
                    }
                    await package.SaveAsync();
                }
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error exporting persons to Excel");
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
        }
    }
}
