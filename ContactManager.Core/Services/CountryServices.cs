using DTO;
using Entities;
using Exceptions;
using Interfaces;
using IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class CountryServices : IcountryServices
    {
        private readonly ICountryRepo _countryRepo;
        private readonly ILogger<CountryServices> _logger;

        public CountryServices(ICountryRepo countryRepo, ILogger<CountryServices> logger)
        {
            _countryRepo = countryRepo ?? throw new ArgumentNullException(nameof(countryRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest countryAddRequest)
        {
            try
            {
                if (countryAddRequest == null)
                    throw new ArgumentNullException(nameof(countryAddRequest));
                if (string.IsNullOrWhiteSpace(countryAddRequest.CountryName))
                    throw new ArgumentException("Country name cannot be empty", nameof(countryAddRequest.CountryName));

                _logger.LogInformation("Adding a new country: {CountryName}", countryAddRequest.CountryName);

                Country newCountry = countryAddRequest.ToCountry();
                var country = await _countryRepo.AddCountry(newCountry);

                _logger.LogInformation("Country {CountryName} added successfully", country.CountryName);
                return country.ToCountryResponse();
            }
            // catch (Exception ex)

            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all countries");
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }

        }

        public async Task<List<string>> ExtractCountryExcelFile(IFormFile file)
        {
            var errors = new List<string>();
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Uploaded file is empty", nameof(file));

                _logger.LogInformation("Processing uploaded Excel file: {FileName}", file.FileName);

                using (var stream = file.OpenReadStream())
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    List<Country> existingCountries = await _countryRepo.GetAllCountries();

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var countryName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(countryName))
                        {
                            errors.Add($"Row {row}: Country name is empty");
                            continue;
                        }

                        if (existingCountries.Any(x => x.CountryName.Equals(countryName, StringComparison.OrdinalIgnoreCase)))
                        {
                            errors.Add($"Row {row}: Country '{countryName}' already exists");
                        }
                        else
                        {
                            var newCountry = new Country { CountryName = countryName, CountryId = Guid.NewGuid() };
                            await _countryRepo.AddCountry(newCountry);
                            _logger.LogInformation("Added new country from Excel: {CountryName}", countryName);
                        }
                    }
                }
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error occurred while extracting country data from Excel file");
      
                errors.Add("An error occurred while processing the file. Please check the log for details.");
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
            return errors;
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            try
            {
                var countries = await _countryRepo.GetAllCountries();
           
                if (countries == null || countries.Count == 0)
                {
                    _logger.LogWarning("No countries found in the database");
                    throw new CustomExceptions("No countries found in the database"); // my custom exception to handle the error
                }
                return countries.Select(x => x.ToCountryResponse()).ToList();
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all countries");
                throw new CustomExceptions( ex.Message,ex.InnerException);
            }
        }

        public async Task<CountryResponse?> GetCountryById(Guid? id)
        {
            try
            {
                if (id == null || id == Guid.Empty)
                    throw new ArgumentException("Invalid country ID", nameof(id));

                _logger.LogInformation("Retrieving country with ID: {CountryId}", id);
                var country = await _countryRepo.GetCountryById(id);
                if (country == null)
                {
                    _logger.LogWarning("Country with ID {CountryId} not found", id);
                    return null;
                }
                return country.ToCountryResponse();
            }
            catch (CustomExceptions ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving country by ID");
                throw new CustomExceptions(ex.Message, ex.InnerException);
            }
        }
    }
}
